﻿namespace Treebank.Annotator.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Domain;
    using Events;
    using Graph;
    using Graph.Algos;
    using GraphX.Controls;
    using GraphX.Controls.Models;
    using GraphX.PCL.Common.Enums;
    using GraphX.PCL.Logic.Helpers;
    using Mappers.Configuration;
    using Prism.Events;
    using Treebank.Events;
    using ViewModels;
    using Wrapper;
    using Point = GraphX.Measure.Point;

    public partial class SentenceEditorView : UserControl, IDisposable, ISentenceEditorView
    {
        private readonly IAppConfig appConfig;

        private readonly SentenceEditorManager editorManager;

        private readonly IEventAggregator eventAggregator;

        private readonly SentenceEditorViewModel viewModel;

        // uniquly identifies the view for Prism events to avoid unwanted calls to subscribers
        private Guid viewUniqueId = Guid.NewGuid();

        private Dictionary<VertexControl, int> numberOfEdgesPerVertexControl = new Dictionary<VertexControl, int>();

        private SenteceGraphOperationMode operationMode = SenteceGraphOperationMode.Select;

        private VertexControl sourceVertexControl;

        public SentenceEditorView(IEventAggregator eventAggregator, IAppConfig appConfig)
        {
            InitializeComponent();
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }

            if (appConfig == null)
            {
                throw new ArgumentNullException("appConfig");
            }

            editorManager = new SentenceEditorManager(GgArea, GgZoomCtrl);
            this.eventAggregator = eventAggregator;
            this.appConfig = appConfig;
        }

        public SentenceEditorView(
            SentenceEditorViewModel sentenceEditorViewModel,
            IEventAggregator eventAggregator,
            IAppConfig appConfig)
            : this(eventAggregator, appConfig)
        {
            if (sentenceEditorViewModel == null)
            {
                throw new ArgumentNullException("sentenceEditorViewModel");
            }

            Loaded += SentenceEditorView_Loaded;
            viewModel = sentenceEditorViewModel;
            viewModel.ViewId = viewUniqueId;
            DataContext = viewModel;

            GgArea.ShowAllEdgesArrows();
            GgArea.ShowAllEdgesLabels();
            GgZoomCtrl.MouseLeftButtonUp += GgZoomCtrlMouseLeftButtonUp;
            GgArea.VertexSelected += GgAreaVertexSelected;
            GgArea.EdgeSelected += GgAreaEdgeSelected;
            GgArea.GenerateGraphFinished += GgAreaGenerateGraphFinished;
            GgArea.EdgeLabelFactory = new DefaultEdgelabelFactory();

            eventAggregator.GetEvent<GenerateGraphEvent>().Subscribe(OnGenerateGraph);
            eventAggregator.GetEvent<SetSentenceEditModeEvent>().Subscribe(OnSetSentenceEditMode);
            eventAggregator.GetEvent<AddWordVertexEvent>().Subscribe(OnAddWordVertexControl);
            eventAggregator.GetEvent<ZoomOnWordVertexEvent>().Subscribe(OnZoomOnWordVertex);
            eventAggregator.GetEvent<ZoomToFillEvent>().Subscribe(ZoomToFill);
        }

        public Guid ViewId
        {
            get { return viewUniqueId; }
            set { viewUniqueId = value; }
        }

        public Definition CurrentConfiguration
        {
            get { return viewModel.SelectedGraphConfiguration; }
        }

        public void Dispose()
        {
            if (editorManager != null)
            {
                editorManager.Dispose();
            }

            if (GgArea != null)
            {
                GgArea.Dispose();
            }
        }

        private void SentenceEditorView_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.PopulateWords();
            viewModel.CreateSentenceGraph();
            viewModel.SetLayoutAlgorithm(viewModel.SentenceGraphLogicCore);
            GgArea.LogicCore = viewModel.SentenceGraphLogicCore;
            DisplayGraph();
            ZoomToFill();
        }

        private void ZoomToFill(Guid viewId)
        {
            if (viewId == viewUniqueId)
            {
                ZoomToFill();
            }
        }

        private void ZoomToFill()
        {
            GgZoomCtrl.ZoomToFill();
            GgZoomCtrl.Mode = ZoomControlModes.Custom;
        }

        private void OnZoomOnWordVertex(ZoomOnWordIdentifier identifier)
        {
            if (identifier.ViewId == ViewId)
            {
                if (!GgArea.VertexList.Any())
                {
                    return;
                }

                var vertexControl = GgArea.VertexList.Values.FirstOrDefault(
                    vc =>
                    {
                        var wordVertex = vc.Vertex as WordVertex;
                        return (wordVertex != null)
                               && (wordVertex.WordWrapper.GetAttributeByName("id") == identifier.WordId);
                    });

                if (vertexControl != null)
                {
                    const int Offset = 100;
                    var pos = vertexControl.GetPosition();
                    GgZoomCtrl.ZoomToContent(
                        new Rect(
                            pos.X - Offset,
                            pos.Y - Offset,
                            vertexControl.ActualWidth + Offset*2,
                            vertexControl.ActualHeight + Offset*3));

                    GgArea.VertexList.ForEach(pair => { HighlightBehaviour.SetHighlighted(pair.Value, false); });

                    HighlightBehaviour.SetHighlighted(vertexControl, true);
                }
            }
        }

        private void OnGenerateGraph(Guid viewId)
        {
            if (viewId == viewUniqueId)
            {
                viewModel.CreateSentenceGraph();
                viewModel.SetLayoutAlgorithm(viewModel.SentenceGraphLogicCore);
                GgArea.LogicCore = viewModel.SentenceGraphLogicCore;
                DisplayGraph();
                viewModel.PopulateWords();
            }
        }

        private void AddVertexConnectionPoints()
        {
            numberOfEdgesPerVertexControl = new Dictionary<VertexControl, int>();

            foreach (var edgeControl in GgArea.EdgesList)
            {
                if (numberOfEdgesPerVertexControl.ContainsKey(edgeControl.Value.Source))
                {
                    numberOfEdgesPerVertexControl[edgeControl.Value.Source]++;
                }
                else
                {
                    numberOfEdgesPerVertexControl.Add(edgeControl.Value.Source, 1);
                }

                if (numberOfEdgesPerVertexControl.ContainsKey(edgeControl.Value.Target))
                {
                    numberOfEdgesPerVertexControl[edgeControl.Value.Target]++;
                }
                else
                {
                    numberOfEdgesPerVertexControl.Add(edgeControl.Value.Target, 1);
                }
            }

            // var resourceDictionary = new ResourceDictionary
            // {
            // Source =
            // new Uri(
            // "/Treebank.Annotator;component/Templates/SentenceEditorViewTemplates.xaml",
            // UriKind.RelativeOrAbsolute)
            // };
            foreach (var pair in numberOfEdgesPerVertexControl)
            {
                var vertex = pair.Key;
                for (var i = 1; i < pair.Value; i++)
                {
                    var newVcp = new StaticVertexConnectionPoint
                    {
                        Id = 1 + i,
                        Margin = new Thickness(2, 0, 0, 0),
                        Shape = VertexShape.Circle

                        // Style = resourceDictionary["CirclePath"] as Style
                    };
                    var cc = new Border
                    {
                        Margin = new Thickness(2, 0, 0, 0),
                        Padding = new Thickness(0),
                        Child = newVcp
                    };
                    if (vertex.VCPRoot == null)
                    {
                        break;
                    }

                    vertex.VCPRoot.Children.Add(cc);
                    vertex.VertexConnectionPointsList.Add(newVcp);
                }
            }
        }

        private void AddEdgesBetweenVertexConnectionPoints(
            Dictionary<VertexControl, int> numberOfEdgesPerVertexControlParam)
        {
            if (!GgArea.EdgesList.Any())
            {
                return;
            }

            var occupiedVcPsPerVertex = new Dictionary<VertexControl, int[]>();

            foreach (var vertexControl in GgArea.VertexList)
            {
                int edgePerVertex;
                if (numberOfEdgesPerVertexControlParam.TryGetValue(vertexControl.Value, out edgePerVertex))
                {
                    occupiedVcPsPerVertex.Add(vertexControl.Value, new int[edgePerVertex + 1]);
                }
            }

            var edgeGaps = ComputeDistancesBetweenEdgeVertices();
            var sortedEdgeGaps = edgeGaps.ToList();
            sortedEdgeGaps.Sort((left, right) => left.Value.CompareTo(right.Value));

            var vertexPositions = GgArea.GetVertexPositions();
            var offset = -25;
            for (var j = 0; j < sortedEdgeGaps.Count; j++)
            {
                var pair = sortedEdgeGaps[j];
                var edgeControl = GgArea.EdgesList[pair.Key];

                var source = edgeControl.Source;
                var target = edgeControl.Target;

                if ((j + 1 < sortedEdgeGaps.Count) && (sortedEdgeGaps[j].Value != sortedEdgeGaps[j + 1].Value))
                {
                    offset -= 25;
                }
                else if (j + 1 == sortedEdgeGaps.Count)
                {
                    offset -= 25;
                }

                var nextVcPid = 1;

                if (vertexPositions[pair.Key.Source].X <= vertexPositions[pair.Key.Target].X)
                {
                    for (var i = occupiedVcPsPerVertex[source].Length; i >= 1; i--)
                    {
                        if (occupiedVcPsPerVertex[source][i - 1] != 1)
                        {
                            occupiedVcPsPerVertex[source][i - 1] = 1;
                            nextVcPid = i - 1;
                            break;
                        }
                    }

                    nextVcPid = nextVcPid <= 0 ? 1 : nextVcPid;
                    pair.Key.SourceConnectionPointId = nextVcPid;

                    nextVcPid = 1;

                    for (var i = 1; i < occupiedVcPsPerVertex[target].Length; i++)
                    {
                        if (occupiedVcPsPerVertex[target][i] != 1)
                        {
                            occupiedVcPsPerVertex[target][i] = 1;
                            nextVcPid = i;
                            break;
                        }
                    }

                    nextVcPid = nextVcPid <= 0 ? 1 : nextVcPid;
                    pair.Key.TargetConnectionPointId = nextVcPid;

                    var sourceVcp =
                        source.VertexConnectionPointsList.FirstOrDefault(
                            vcp => vcp.Id == pair.Key.SourceConnectionPointId);

                    if (sourceVcp == null)
                    {
                        continue;
                    }

                    var sourcePos = sourceVcp.RectangularSize;

                    var targetVcp =
                        target.VertexConnectionPointsList.FirstOrDefault(
                            vcp => vcp.Id == pair.Key.TargetConnectionPointId);

                    if (targetVcp == null)
                    {
                        continue;
                    }

                    var targetPos = targetVcp.RectangularSize;

                    pair.Key.RoutingPoints = new[]
                    {
                        new Point(sourcePos.X, sourcePos.Y),
                        new Point(sourcePos.X, sourcePos.Y + offset),
                        new Point(targetPos.X, sourcePos.Y + offset),
                        new Point(targetPos.X, targetPos.Y)
                    };
                }
                else
                {
                    for (var i = occupiedVcPsPerVertex[target].Length; i >= 1; i--)
                    {
                        if (occupiedVcPsPerVertex[target][i - 1] != 1)
                        {
                            occupiedVcPsPerVertex[target][i - 1] = 1;
                            nextVcPid = i - 1;
                            break;
                        }
                    }

                    nextVcPid = nextVcPid <= 0 ? 1 : nextVcPid;
                    pair.Key.TargetConnectionPointId = nextVcPid;

                    nextVcPid = 1;

                    for (var i = 1; i < occupiedVcPsPerVertex[source].Length; i++)
                    {
                        if (occupiedVcPsPerVertex[source][i] != 1)
                        {
                            occupiedVcPsPerVertex[source][i] = 1;
                            nextVcPid = i;
                            break;
                        }
                    }

                    nextVcPid = nextVcPid <= 0 ? 1 : nextVcPid;
                    pair.Key.SourceConnectionPointId = nextVcPid;

                    var sourceVcp =
                        source.VertexConnectionPointsList.FirstOrDefault(
                            vcp => vcp.Id == pair.Key.SourceConnectionPointId);

                    if (sourceVcp == null)
                    {
                        continue;
                    }

                    var sourcePos = sourceVcp.RectangularSize;

                    var targetVcp =
                        target.VertexConnectionPointsList.FirstOrDefault(
                            vcp => vcp.Id == pair.Key.TargetConnectionPointId);
                    if (targetVcp == null)
                    {
                        continue;
                    }

                    var targetPos = targetVcp.RectangularSize;

                    pair.Key.RoutingPoints = new[]
                    {
                        new Point(sourcePos.X, sourcePos.Y),
                        new Point(sourcePos.X, sourcePos.Y + offset),
                        new Point(targetPos.X, sourcePos.Y + offset),
                        new Point(targetPos.X, targetPos.Y)
                    };
                }
            }
        }

        private Dictionary<WordEdge, long> ComputeDistancesBetweenEdgeVertices()
        {
            var distancesBetweenEdgeNodes = new Dictionary<WordEdge, long>();

            foreach (var edge in GgArea.EdgesList.Keys)
            {
                var distance = (edge.Source.ID - edge.Target.ID)*(edge.Source.ID - edge.Target.ID);

                distancesBetweenEdgeNodes.Add(edge, distance);
            }

            return distancesBetweenEdgeNodes;
        }

        private void GgAreaEdgeSelected(object sender, EdgeSelectedEventArgs args)
        {
            if ((args.MouseArgs.LeftButton == MouseButtonState.Pressed)
                && (operationMode == SenteceGraphOperationMode.Delete))
            {
                var edge = args.EdgeControl.Edge as WordEdge;
                if (edge != null)
                {
                    GgArea.RemoveEdge(edge, true);
                    var targetVertex = edge.Target;
                    targetVertex.WordWrapper.SetAttributeByName(CurrentConfiguration.Edge.SourceVertexAttributeName, "0");
                    targetVertex.WordWrapper.SetAttributeByName(
                        CurrentConfiguration.Edge.LabelAttributeName,
                        string.Empty);

                    viewModel.InvalidateCommands();
                }
            }
        }

        private void OnAddWordVertexControl(WordWrapper word)
        {
            var vertexControl = AddWordVertexControl(word);
            var headWordId = word.GetAttributeByName(CurrentConfiguration.Edge.SourceVertexAttributeName);
            var headWordVertexControl =
                GgArea.VertexList.Where(
                    p =>
                        p.Key.WordWrapper.GetAttributeByName(CurrentConfiguration.Edge.TargetVertexAttributeName)
                            .Equals(headWordId))
                    .Select(p => p.Value)
                    .SingleOrDefault();

            if (headWordVertexControl != null)
            {
                sourceVertexControl = headWordVertexControl;
                CreateEdgeControl(vertexControl);
            }

            DisplayGraph();
            ZoomToFill();
        }

        private void OnSetSentenceEditMode(SetSenteceGraphOperationModeRequest setSenteceGraphOperationModeRequest)
        {
            if ((butDelete.IsChecked == true)
                && (setSenteceGraphOperationModeRequest.Mode == SenteceGraphOperationMode.Delete))
            {
                butEdit.IsChecked = false;
                butSelect.IsChecked = false;
                GgZoomCtrl.Cursor = Cursors.Help;
                viewModel.SenteceGraphOperationMode = SenteceGraphOperationMode.Delete;
                operationMode = SenteceGraphOperationMode.Delete;
                ClearEditMode();
                ClearSelectMode();
                GgArea.SetVerticesDrag(false);
                return;
            }

            if ((butEdit.IsChecked == true)
                && (setSenteceGraphOperationModeRequest.Mode == SenteceGraphOperationMode.Edit))
            {
                butDelete.IsChecked = false;
                butSelect.IsChecked = false;
                GgZoomCtrl.Cursor = Cursors.Pen;
                viewModel.SenteceGraphOperationMode = SenteceGraphOperationMode.Edit;
                operationMode = SenteceGraphOperationMode.Edit;
                ClearSelectMode();
                GgArea.SetVerticesDrag(false);
                return;
            }

            if ((butSelect.IsChecked == true)
                && (setSenteceGraphOperationModeRequest.Mode == SenteceGraphOperationMode.Select))
            {
                butEdit.IsChecked = false;
                butDelete.IsChecked = false;
                GgZoomCtrl.Cursor = Cursors.Hand;
                viewModel.SenteceGraphOperationMode = SenteceGraphOperationMode.Select;
                operationMode = SenteceGraphOperationMode.Select;
                ClearEditMode();
                GgArea.SetVerticesDrag(true, true);
            }
            else
            {
                GgArea.SetVerticesDrag(false);
            }
        }

        private void ClearSelectMode(bool soft = false)
        {
            GgArea.VertexList.Values.Where(DragBehaviour.GetIsTagged).ToList().ForEach(
                a =>
                {
                    HighlightBehaviour.SetHighlighted(a, false);
                    DragBehaviour.SetIsTagged(a, false);
                });

            if (!soft)
            {
                GgArea.SetVerticesDrag(false);
            }
        }

        private void ClearEditMode()
        {
            if (sourceVertexControl != null)
            {
                HighlightBehaviour.SetHighlighted(sourceVertexControl, false);
            }

            editorManager.DestroyVirtualEdge();
            sourceVertexControl = null;
        }

        private void GgZoomCtrlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ClearAllSelectedVertices();
        }

        private void ClearAllSelectedVertices()
        {
            GgArea.VertexList.ForEach(
                pair =>
                {
                    if (DragBehaviour.GetIsTagged(pair.Value))
                    {
                        HighlightBehaviour.SetHighlighted(pair.Value, false);
                        DragBehaviour.SetIsTagged(pair.Value, false);
                    }
                });
        }

        private void GgAreaVertexSelected(object sender, VertexSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            switch (viewModel.SenteceGraphOperationMode)
            {
                case SenteceGraphOperationMode.Edit :
                    CreateEdgeControl(args.VertexControl);
                    break;
                case SenteceGraphOperationMode.Delete :
                    SafeRemoveVertex(args.VertexControl);
                    break;
                case SenteceGraphOperationMode.Select :
                    SelectVertex(args.VertexControl);
                    break;
            }
        }

        private void CreateEdgeControl(VertexControl targetVertexControl)
        {
            if (sourceVertexControl == null)
            {
                editorManager.CreateVirtualEdge(targetVertexControl, targetVertexControl.GetPosition());
                sourceVertexControl = targetVertexControl;
                HighlightBehaviour.SetHighlighted(sourceVertexControl, true);
                return;
            }

            if (Equals(sourceVertexControl, targetVertexControl))
            {
                return;
            }

            var wordPrototype = appConfig.Elements.OfType<Word>().Single();

            var addEdgeDialog =
                new AddEdgeWindow(
                    new AddEdgeViewModel(
                        wordPrototype.Attributes.Single(
                            a => a.Name.Equals(CurrentConfiguration.Edge.LabelAttributeName))));

            if (addEdgeDialog.ShowDialog().GetValueOrDefault())
            {
                var edgeLabelText = string.Empty;
                var dataContext = addEdgeDialog.DataContext as AddEdgeViewModel;
                if (dataContext != null)
                {
                    edgeLabelText = dataContext.Attributes.First().Value;
                }

                var sourceWordVertex = sourceVertexControl.Vertex as WordVertex;
                var targetWordVertex = targetVertexControl.Vertex as WordVertex;

                if ((sourceWordVertex == null) || (targetWordVertex == null))
                {
                    return;
                }

                targetWordVertex.WordWrapper.SetAttributeByName(
                    CurrentConfiguration.Edge.SourceVertexAttributeName,
                    sourceWordVertex.WordWrapper.GetAttributeByName(CurrentConfiguration.Edge.TargetVertexAttributeName));
                targetWordVertex.WordWrapper.SetAttributeByName(
                    CurrentConfiguration.Edge.LabelAttributeName,
                    edgeLabelText);

                var data = new WordEdge((WordVertex) sourceVertexControl.Vertex, targetWordVertex)
                {
                    Text = edgeLabelText
                };

                var ec = new EdgeControl(sourceVertexControl, targetVertexControl, data);
                GgArea.InsertEdgeAndData(data, ec, 0, true);

                HighlightBehaviour.SetHighlighted(sourceVertexControl, false);
                sourceVertexControl = null;
                editorManager.DestroyVirtualEdge();
            }

            viewModel.InvalidateCommands();
        }

        private VertexControl AddWordVertexControl(WordWrapper wordWrapper)
        {
            var vertex = new WordVertex(wordWrapper, CurrentConfiguration.Vertex.LabelAttributeName);
            var vertexControl = new VertexControl(vertex);
            GgArea.AddVertexAndData(vertex, vertexControl, true);
            return vertexControl;
        }

        private void SafeRemoveVertex(VertexControl vc)
        {
            var wordToRemove = vc.Vertex as WordVertex;

            if (wordToRemove != null)
            {
                GgArea.RemoveVertexAndEdges(wordToRemove);
                viewModel.Sentence.Words.Remove(wordToRemove.WordWrapper);

                foreach (var word in viewModel.Sentence.Words)
                {
                    if (word.GetAttributeByName(CurrentConfiguration.Edge.SourceVertexAttributeName)
                        == wordToRemove.WordWrapper.GetAttributeByName(CurrentConfiguration.Edge.TargetVertexAttributeName))
                    {
                        word.SetAttributeByName(
                            CurrentConfiguration.Edge.SourceVertexAttributeName,
                            wordToRemove.WordWrapper.GetAttributeByName(CurrentConfiguration.Edge.SourceVertexAttributeName));
                    }
                }

                viewModel.CreateSentenceGraph();
                viewModel.SetLayoutAlgorithm(viewModel.SentenceGraphLogicCore);
                GgArea.LogicCore = viewModel.SentenceGraphLogicCore;
                viewModel.PopulateWords();
                DisplayGraph();
                ZoomToFill();
            }
        }

        private void SelectVertex(VertexControl vertexControl)
        {
            var vertex = vertexControl.Vertex as WordVertex;
            if (vertex != null)
            {
                eventAggregator.GetEvent<ChangeAttributesEditorViewModel>()
                    .Publish(
                        new ElementAttributeEditorViewModel(eventAggregator, viewModel.ViewId)
                        {
                            Attributes =
                                vertex
                                    .WordWrapper
                                    .Attributes
                        });
            }

            ClearAllSelectedVertices();

            if (DragBehaviour.GetIsTagged(vertexControl))
            {
                HighlightBehaviour.SetHighlighted(vertexControl, false);
                DragBehaviour.SetIsTagged(vertexControl, false);
            }
            else
            {
                HighlightBehaviour.SetHighlighted(vertexControl, true);
                DragBehaviour.SetIsTagged(vertexControl, true);
            }
        }

        private void GgAreaGenerateGraphFinished(object sender, EventArgs e)
        {
            if ((viewModel.SelectedLayoutAlgorithmType == GraphLayoutAlgorithmTypeEnum.DiagonalLiniar)
                || (viewModel.SelectedLayoutAlgorithmType == GraphLayoutAlgorithmTypeEnum.Liniar))
            {
                AddVertexConnectionPoints();
            }
            else
            {
                RemoveVertexConnectionPoints();
            }

            foreach (var vertexControl in GgArea.VertexList.Values)
            {
                vertexControl.VertexConnectionPointsList.ForEach(a => a.Shape = VertexShape.Circle);
                HighlightBehaviour.SetHighlightControl(vertexControl, GraphControlType.VertexAndEdge);
                HighlightBehaviour.SetIsHighlightEnabled(vertexControl, true);
                HighlightBehaviour.SetHighlightEdges(vertexControl, EdgesType.All);
            }

            foreach (var item in GgArea.EdgesList)
            {
                HighlightBehaviour.SetHighlightControl(item.Value, GraphControlType.VertexAndEdge);
                HighlightBehaviour.SetIsHighlightEnabled(item.Value, true);
                HighlightBehaviour.SetHighlightEdges(item.Value, EdgesType.All);
            }

            GgArea.VertexList.Values.ForEach(vc => vc.SetConnectionPointsVisibility(true));
        }

        private void RemoveVertexConnectionPoints()
        {
            foreach (var vertexControl in GgArea.VertexList.Values)
            {
                if ((vertexControl.VCPRoot != null) && (vertexControl.VCPRoot.Children != null)
                    && (vertexControl.VCPRoot.Children.Count > 1))
                {
                    vertexControl.VCPRoot.Children.RemoveRange(1, vertexControl.VCPRoot.Children.Count - 1);
                }
            }

            foreach (var edgeControl in GgArea.EdgesList.Keys)
            {
                edgeControl.SourceConnectionPointId = null;
                edgeControl.TargetConnectionPointId = null;
            }
        }

        private void DisplayGraph()
        {
            GgArea.GenerateGraph(); // this will trigger and execute GgArea_GenerateGraphFinished
            GgArea.RelayoutGraph(true);

            if ((viewModel.SelectedLayoutAlgorithmType == GraphLayoutAlgorithmTypeEnum.DiagonalLiniar)
                || (viewModel.SelectedLayoutAlgorithmType == GraphLayoutAlgorithmTypeEnum.Liniar))
            {
                AddEdgesBetweenVertexConnectionPoints(numberOfEdgesPerVertexControl);
            }

            GgArea.UpdateAllEdges(true);
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.CreateSentenceGraph();
            viewModel.SetLayoutAlgorithm(viewModel.SentenceGraphLogicCore);
            GgArea.LogicCore = viewModel.SentenceGraphLogicCore;
            DisplayGraph();
            ZoomToFill();
        }
    }
}