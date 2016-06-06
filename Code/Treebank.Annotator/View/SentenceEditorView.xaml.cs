﻿namespace Treebank.Annotator.View
{
    using System;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Domain;
    using Events;
    using Graph;
    using GraphX.Controls;
    using GraphX.Controls.Models;
    using GraphX.PCL.Common.Enums;
    using GraphX.PCL.Logic.Helpers;
    using Mappers.Configuration;
    using Prism.Events;
    using Treebank.Events;
    using ViewModels;
    using Wrapper;

    public partial class SentenceEditorView : UserControl, IDisposable
    {
        private readonly IAppConfig appConfig;

        private readonly Definition currentDefinition;

        private readonly SentenceEditorManager editorManager;

        private readonly IEventAggregator eventAggregator;

        private readonly SentenceEditorViewModel viewModel;

        private VertexControl fromVertexControl;

        private SenteceGraphOperationMode operationMode = SenteceGraphOperationMode.Select;

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
            IAppConfig appConfig,
            Definition definition = null)
            : this(eventAggregator, appConfig)
        {
            if (sentenceEditorViewModel == null)
            {
                throw new ArgumentNullException("sentenceEditorViewModel");
            }

            viewModel = sentenceEditorViewModel;
            DataContext = viewModel;

            if (definition == null)
            {
                currentDefinition = appConfig.Definitions.Any()
                    ? this.appConfig.Definitions.First()
                    : MotherObjects.DefaultDefinition;
            }
            else
            {
                currentDefinition = definition;
            }

            viewModel.CreateSentenceGraph();
            GgZoomCtrl.MouseLeftButtonUp += GgZoomCtrl_MouseLeftButtonUp;
            GgArea.VertexSelected += GgArea_VertexSelected;
            GgArea.EdgeSelected += GgArea_EdgeSelected;
            GgArea.RelayoutFinished += GgArea_RelayoutFinished;
            GgArea.GenerateGraphFinished += GgArea_GenerateGraphFinished;
            GgArea.EdgeLabelFactory = new DefaultEdgelabelFactory();

            viewModel.EventAggregator.GetEvent<RelayoutGraphEvent>().Subscribe(OnRelayoutGraph);
            viewModel.EventAggregator.GetEvent<SetSentenceEditModeEvent>().Subscribe(OnSetSentenceEditMode);
            viewModel.EventAggregator.GetEvent<AddWordVertexEvent>().Subscribe(OnAddWordVertexControl);
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

        private void GgArea_EdgeSelected(object sender, EdgeSelectedEventArgs args)
        {
            if ((args.MouseArgs.LeftButton == MouseButtonState.Pressed)
                && (operationMode == SenteceGraphOperationMode.Delete))
            {
                GgArea.RemoveEdge(args.EdgeControl.Edge as WordEdge, true);
            }
        }

        private void OnAddWordVertexControl(WordWrapper word)
        {
            var vertexControl = AddWordVertexControl(word);
            var headWordId = word.GetAttributeByName(currentDefinition.Vertex.FromAttributeName);
            var headWordVertexControl =
                GgArea.VertexList.Where(
                    p =>
                        p.Key.WordWrapper.GetAttributeByName(currentDefinition.Vertex.ToAttributeName)
                            .Equals(headWordId))
                    .Select(p => p.Value)
                    .SingleOrDefault();

            if (headWordVertexControl != null)
            {
                fromVertexControl = headWordVertexControl;
                CreateEdgeControl(vertexControl);
            }

            DisplayGraph();
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
            if (fromVertexControl != null)
            {
                HighlightBehaviour.SetHighlighted(fromVertexControl, false);
            }

            editorManager.DestroyVirtualEdge();
            fromVertexControl = null;
        }

        private void GgZoomCtrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        private void GgArea_VertexSelected(object sender, VertexSelectedEventArgs args)
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

        private void CreateEdgeControl(VertexControl toVertexControl)
        {
            if (fromVertexControl == null)
            {
                editorManager.CreateVirtualEdge(toVertexControl, toVertexControl.GetPosition());
                fromVertexControl = toVertexControl;
                HighlightBehaviour.SetHighlighted(fromVertexControl, true);
                return;
            }

            if (Equals(fromVertexControl, toVertexControl))
            {
                return;
            }

            var wordPrototype = appConfig.Elements.OfType<Word>().Single();
            var addEdgeDialog =
                new AddEdgeWindow(
                    new AddEdgeViewModel(
                        wordPrototype.Attributes.Single(a => a.Name.Equals(currentDefinition.Edge.LabelAttributeName))));
            if (addEdgeDialog.ShowDialog().GetValueOrDefault())
            {
                var edgeLabelText = string.Empty;
                var dataContext = addEdgeDialog.DataContext as AddEdgeViewModel;
                if (dataContext != null)
                {
                    edgeLabelText = dataContext.Attributes.First().Value;
                }

                var data = new WordEdge((WordVertex) fromVertexControl.Vertex, (WordVertex) toVertexControl.Vertex)
                {
                    Text
                        =
                        edgeLabelText
                };

                var ec = new EdgeControl(fromVertexControl, toVertexControl, data);
                GgArea.InsertEdgeAndData(data, ec, 0, true);

                HighlightBehaviour.SetHighlighted(fromVertexControl, false);
                fromVertexControl = null;
                editorManager.DestroyVirtualEdge();
            }
        }

        private VertexControl AddWordVertexControl(WordWrapper wordWrapper)
        {
            var vertex = new WordVertex(wordWrapper, currentDefinition.Vertex.LabelAttributeName);
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
                    if (word.GetAttributeByName(currentDefinition.Vertex.FromAttributeName)
                        == wordToRemove.WordWrapper.GetAttributeByName(currentDefinition.Vertex.ToAttributeName))
                    {
                        word.SetAttributeByName(
                            currentDefinition.Vertex.FromAttributeName,
                            wordToRemove.WordWrapper.GetAttributeByName(currentDefinition.Vertex.FromAttributeName));
                    }
                }

                GgArea.RemoveAllEdges();
                GgArea.RemoveAllVertices();

                viewModel.CreateSentenceGraph();
                viewModel.PopulateWords();

                GgArea.InvalidateVisual();
            }
        }

        private void SelectVertex(VertexControl vertexControl)
        {
            var vertex = vertexControl.Vertex as WordVertex;
            if (vertex != null)
            {
                eventAggregator.GetEvent<ChangeAttributesEditorViewModel>()
                    .Publish(new ElementAttributeEditorViewModel(eventAggregator)
                    {
                        Attributes = vertex.WordWrapper.Attributes
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

        private void OnRelayoutGraph(bool relayout)
        {
            if (relayout)
            {
                DisplayGraph();
                viewModel.PopulateWords();
            }
        }

        private void GgArea_GenerateGraphFinished(object sender, EventArgs e)
        {
            if (!GgArea.EdgesList.Any())
            {
                GgArea.GenerateAllEdges();
            }

            GgArea.ShowAllEdgesArrows();
            GgArea.ShowAllEdgesLabels();

            GgZoomCtrl.ZoomToFill();
            GgZoomCtrl.Mode = ZoomControlModes.Custom;


            foreach (var item in GgArea.VertexList)
            {
                HighlightBehaviour.SetHighlightControl(item.Value, GraphControlType.VertexAndEdge);
                HighlightBehaviour.SetIsHighlightEnabled(item.Value, true);
                HighlightBehaviour.SetHighlightEdges(item.Value, EdgesType.All);
            }

            foreach (var item in GgArea.EdgesList)
            {
                HighlightBehaviour.SetHighlightControl(item.Value, GraphControlType.VertexAndEdge);
                HighlightBehaviour.SetIsHighlightEnabled(item.Value, true);
                HighlightBehaviour.SetHighlightEdges(item.Value, EdgesType.All);
            }
        }

        private void GgArea_RelayoutFinished(object sender, EventArgs e)
        {
            GgZoomCtrl.ZoomToFill();
            GgZoomCtrl.Mode = ZoomControlModes.Custom;
        }

        private void DisplayGraph()
        {
            GgArea.GenerateGraph();
            GgArea.ShowAllEdgesArrows();
            GgArea.ShowAllEdgesLabels();
            GgArea.RelayoutGraph(true);
            if (!GgArea.EdgesList.Any())
            {
                GgArea.GenerateAllEdges();
            }

            GgZoomCtrl.ZoomToFill();
            GgZoomCtrl.Mode = ZoomControlModes.Custom;
        }
    }
}