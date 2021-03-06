﻿namespace Treebank.Annotator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Commands;
    using Domain;
    using Graph;
    using GraphX.PCL.Common.Enums;
    using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
    using Mappers;
    using Mappers.Algos;
    using Mappers.Configuration;
    using Prism.Events;
    using Treebank.Events;
    using View;
    using View.Services;
    using Wrapper;
    using Wrapper.Base;

    public class SentenceEditorViewModel : Observable
    {
        private readonly IAppConfig appConfig;
        private readonly DataStructure dataStructure;

        protected GraphBuilder GraphBuilder;

        private readonly IShowInfoMessage showMessage;

        private ObservableCollection<GraphLayoutAlgorithmTypeEnum> layoutAlgorithmTypes =
            new ObservableCollection<GraphLayoutAlgorithmTypeEnum>(
                Enum.GetValues(typeof(GraphLayoutAlgorithmTypeEnum)).Cast<GraphLayoutAlgorithmTypeEnum>());

        private StringWrapper leftStringWrapper;

        private SenteceGraphOperationMode operationMode = SenteceGraphOperationMode.Select;

        private SentenceGxLogicCore sentenceLogicCore;

        private SentenceWrapper sentenceWrapper;

        public SentenceEditorViewModel(
            IEventAggregator eventAggregator,
            IAppConfig appConfig,
            DataStructure dataStructure,
            SentenceWrapper sentence,
            IShowInfoMessage showMessage)
        {
            if (sentence == null)
            {
                throw new ArgumentNullException("sentence");
            }

            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }

            if (appConfig == null)
            {
                throw new ArgumentNullException("appConfig");
            }

            if (showMessage == null)
            {
                throw new ArgumentNullException("showMessage");
            }

            if (dataStructure == null)
            {
                throw new ArgumentNullException("dataStructure");
            }

            InitializeCommands();

            EventAggregator = eventAggregator;
            Sentence = sentence;
            this.appConfig = appConfig;
            this.showMessage = showMessage;
            this.dataStructure = dataStructure;
            GraphConfigurations = new ObservableCollection<Definition>(this.appConfig.Definitions);
            SelectedGraphConfiguration = GraphConfigurations.FirstOrDefault();

            var sentenceGraph = new SentenceGraph();
            sentenceLogicCore = new SentenceGxLogicCore {Graph = sentenceGraph};
        }

        public DataStructure DataStructure
        {
            get { return dataStructure; }
        }

        public SenteceGraphOperationMode SenteceGraphOperationMode
        {
            get { return operationMode; }

            set { operationMode = value; }
        }

        public IEventAggregator EventAggregator { get; set; }

        public ICommand CheckIsTreeCommand { get; set; }

        public ICommand LayoutAlgorithmChangedCommand { get; set; }

        public ICommand AddWordCommand { get; set; }

        public ICommand GraphConfigurationChangedCommand { get; set; }

        public ICommand ToggleEditModeCommand { get; set; }
        
        public StringWrapper LeftSentenceInfo
        {
            get { return leftStringWrapper; }
            set
            {
                leftStringWrapper = value;
                OnPropertyChanged();
            }
        }

        public SentenceWrapper Sentence
        {
            get { return sentenceWrapper; }

            set
            {
                sentenceWrapper = value;
                InvalidateCommands();
                OnPropertyChanged();
            }
        }

        public ChangeTrackingCollection<WordEditorViewModel> Words { get; set; }

        public SentenceGxLogicCore SentenceGraphLogicCore
        {
            get { return sentenceLogicCore; }

            set
            {
                sentenceLogicCore = value;
                OnPropertyChanged();
            }
        }

        public GraphLayoutAlgorithmTypeEnum SelectedLayoutAlgorithmType { get; set; }

        public Definition SelectedGraphConfiguration { get; set; }

        public ObservableCollection<GraphLayoutAlgorithmTypeEnum> LayoutAlgorithmTypes
        {
            get { return layoutAlgorithmTypes; }

            set
            {
                layoutAlgorithmTypes = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Definition> GraphConfigurations { get; set; }

        public Guid ViewId { get; set; }

        public ICommand RejectChangesCommand { get; set; }

        public ICommand AcceptChangesCommand { get; set; }

        public void PopulateWords()
        {
            PopulateWords(EventAggregator, sentenceWrapper);
        }

        public virtual void CreateSentenceGraph()
        {
            if (Sentence != null)
            {
                GraphBuilder = new GraphBuilder(appConfig, appConfig.Definitions.First())
                {
                    CurrentGraphDefinition = SelectedGraphConfiguration
                };

                var logicCore = GraphBuilder.SetupGraphLogic(Sentence);
                SentenceGraphLogicCore = logicCore;
            }
        }

        public void SetLayoutAlgorithm(SentenceGxLogicCore logicCore)
        {
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.None;
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            logicCore.ExternalEdgeRoutingAlgorithm = null;
            logicCore.ExternalEdgeRoutingAlgorithm = null;
            logicCore.ExternalOverlapRemovalAlgorithm = null;

            switch (SelectedLayoutAlgorithmType)
            {
                case GraphLayoutAlgorithmTypeEnum.Liniar :
                    logicCore.ExternalLayoutAlgorithm = new LiniarLayoutAlgorithm<WordVertex, WordEdge, SentenceGraph>(
                        logicCore.Graph as SentenceGraph,
                        50);
                    break;
                case GraphLayoutAlgorithmTypeEnum.DiagonalLiniar :
                    logicCore.ExternalLayoutAlgorithm = new LiniarLayoutAlgorithm<WordVertex, WordEdge, SentenceGraph>(
                        logicCore.Graph as SentenceGraph,
                        50,
                        25);
                    break;
                case GraphLayoutAlgorithmTypeEnum.EfficientSugiyama :
                    SetEfficientSugiyamaLayout(logicCore);
                    break;
                default :
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PopulateWords(IEventAggregator eventAggregator, SentenceWrapper sentence)
        {
            var sortedWords = sentence.Words.ToList();
            sortedWords.Sort(Comparison);

            IList<WordEditorViewModel> w =
                sortedWords.Select(word => new WordEditorViewModel(word, eventAggregator, ViewId)).ToList();

            Words = new ChangeTrackingCollection<WordEditorViewModel>(w);
            InvalidateCommands();
            OnPropertyChanged("Words");
        }

        private int Comparison(WordWrapper left, WordWrapper right)
        {
            var leftId = int.Parse(left.GetAttributeByName("id"));
            var rightId = int.Parse(right.GetAttributeByName("id"));

            if (leftId == rightId)
            {
                return 0;
            }

            return leftId > rightId ? 1 : -1;
        }

        private void InitializeCommands()
        {
            LayoutAlgorithmChangedCommand = new DelegateCommand(
                LayoutAlgorithmChangedCommandExecute,
                LayoutAlgorithmChangedCommandCanExecute);
            GraphConfigurationChangedCommand = new DelegateCommand(
                GraphConfigurationChangedCommandExecute,
                GraphConfigurationChangedCommandCanExecute);
            ToggleEditModeCommand = new DelegateCommand(ToggleEditModeCommandExecute, ToggleEditModeCommandCanExecute);
            AddWordCommand = new DelegateCommand(AddWordCommandExecute, AddWordCommandCanExecute);
            CheckIsTreeCommand = new DelegateCommand(CheckIsTreeCommandExecute, CheckIsTreeCommandCanExecute);
            AcceptChangesCommand = new DelegateCommand(AcceptChangesCommandExecute, AcceptChangesCommandCanExecute);
            RejectChangesCommand = new DelegateCommand(RejectChangesCommandExecute, RejectChangesCommandCanExecute);
        }

        private bool RejectChangesCommandCanExecute(object arg)
        {
            return Sentence.IsChanged;
        }

        private void RejectChangesCommandExecute(object obj)
        {
            Sentence.RejectChanges();
            EventAggregator.GetEvent<UpdateAllViewsForSentenceByViewId>().Publish(ViewId);
        }

        private void AcceptChangesCommandExecute(object obj)
        {
            Sentence.AcceptChanges();
            EventAggregator.GetEvent<UpdateAllViewsForSentenceByViewId>().Publish(ViewId);
        }

        private bool AcceptChangesCommandCanExecute(object arg)
        {
            return Sentence.IsChanged;
        }

        public void InvalidateCommands()
        {
            ((DelegateCommand) LayoutAlgorithmChangedCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) GraphConfigurationChangedCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) ToggleEditModeCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) AddWordCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) CheckIsTreeCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) AcceptChangesCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) RejectChangesCommand).RaiseCanExecuteChanged();
        }

        private void CheckIsTreeCommandExecute(object obj)
        {
            var validationResult = new CheckGraphResult();
            var isTree =
                GraphOperations.GetGraph(Sentence.Model, appConfig.Definitions.First(), EventAggregator)
                    .IsTree(validationResult);

            if (!isTree)
            {
                foreach (var disconnectedWordId in validationResult.DisconnectedWordIds)
                {
                    EventAggregator.GetEvent<ValidationExceptionEvent>()
                        .Publish(
                            string.Format(
                                "The word with id: {0}, is not connected to another word.",
                                disconnectedWordId));
                }

                foreach (var cycle in validationResult.Cycles)
                {
                    EventAggregator.GetEvent<ValidationExceptionEvent>()
                        .Publish(
                            string.Format(
                                "The sentence with id {0} has cycle: {1}",
                                Sentence.Id.Value,
                                string.Join(",", cycle)));
                }

                if (validationResult.DisconnectedWordIds.Any() || validationResult.Cycles.Any())
                {
                    EventAggregator.GetEvent<StatusNotificationEvent>()
                        .Publish("Please check warnings in the Output panel.");
                }
            }

            showMessage.ShowInfoMessage(
                string.Format("Graph for sentence with id {0} is tree: {1}", Sentence.Id.Value, isTree),
                MessageBoxButton.OK);
        }

        private bool CheckIsTreeCommandCanExecute(object arg)
        {
            return true;
        }

        private void AddWordCommandExecute(object obj)
        {
            var wordPrototype = ObjectCopier.Clone(dataStructure.Elements.OfType<Word>().Single());
            var wordIds =
                Sentence.Words.Select(
                    w => new Pair {Id = int.Parse(w.GetAttributeByName("id")), Form = w.GetAttributeByName("form")})
                    .ToList();
            var addWordWindow = new AddWordWindow(new AddWordViewModel(wordPrototype, wordIds));

            if (addWordWindow.ShowDialog().GetValueOrDefault())
            {
                var word = ((AddWordViewModel) addWordWindow.DataContext).Word;
                Sentence.Words.Add(word);

                var wordReorderingWindow = new WordReorderingWindow(new WordReorderingViewModel(Sentence));
                if (wordReorderingWindow.ShowDialog().GetValueOrDefault())
                {
                    Sentence.UpdateContent();
                    EventAggregator.GetEvent<UpdateAllViewsForSentenceByViewId>().Publish(ViewId);
                    EventAggregator.GetEvent<ZoomToFillEvent>().Publish(ViewId);
                }
            }

            CreateSentenceGraph();
            PopulateWords(EventAggregator, Sentence);

            InvalidateCommands();
        }

        private bool AddWordCommandCanExecute(object arg)
        {
            return true;
        }

        private bool ToggleEditModeCommandCanExecute(object arg)
        {
            return true;
        }

        private void ToggleEditModeCommandExecute(object obj)
        {
            EventAggregator.GetEvent<SetSentenceEditModeEvent>()
                .Publish(
                    new SetSenteceGraphOperationModeRequest
                    {
                        Mode =
                            obj is SenteceGraphOperationMode
                                ? (SenteceGraphOperationMode) obj
                                : SenteceGraphOperationMode.Select
                    });
        }

        private bool GraphConfigurationChangedCommandCanExecute(object arg)
        {
            return true;
        }

        private void GraphConfigurationChangedCommandExecute(object obj)
        {
            EventAggregator.GetEvent<GenerateGraphEvent>().Publish(ViewId);
            EventAggregator.GetEvent<ZoomToFillEvent>().Publish(ViewId);
        }

        private void LayoutAlgorithmChangedCommandExecute(object obj)
        {
            EventAggregator.GetEvent<GenerateGraphEvent>().Publish(ViewId);
            EventAggregator.GetEvent<ZoomToFillEvent>().Publish(ViewId);
        }

        private bool LayoutAlgorithmChangedCommandCanExecute(object arg)
        {
            return true;
        }

        private void SetEfficientSugiyamaLayout(SentenceGxLogicCore logicCore)
        {
            logicCore.ExternalEdgeRoutingAlgorithm = null;
            logicCore.ExternalLayoutAlgorithm = null;

            var layoutParameters =
                logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.EfficientSugiyama) as
                    EfficientSugiyamaLayoutParameters;

            if (layoutParameters != null)
            {
                layoutParameters.EdgeRouting = SugiyamaEdgeRoutings.Traditional;
                layoutParameters.LayerDistance = layoutParameters.VertexDistance = 75;
                logicCore.EdgeCurvingEnabled = false;
                logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.EfficientSugiyama;
                logicCore.DefaultLayoutAlgorithmParams = layoutParameters;
            }
        }
    }
}