﻿namespace SemanticRelationsResolver.Annotator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using Commands;
    using Domain;
    using Events;
    using Mappers;
    using Prism.Events;
    using View;
    using View.Services;
    using Wrapper;

    public class MainViewModel : Observable
    {
        private string currentStatus;
        private IDocumentMapper documentMapper;

        private Dictionary<string, DocumentWrapper> documentsWrappers;

        private IEventAggregator eventAggregator;

        private IOpenFileDialogService openFileDialogService;

        private ISaveDialogService saveDialogService;

        private DocumentWrapper selectedDocument;

        private ObservableCollection<SentenceEditorView> sentenceEditViewModels;

        public MainViewModel(
            IEventAggregator eventAggregator,
            ISaveDialogService saveDialogService,
            IOpenFileDialogService openFileDialogService,
            IDocumentMapper documentMapper)
        {
            InitializeCommands();

            InitializeServices(eventAggregator, saveDialogService, openFileDialogService, documentMapper);

            SubscribeToEvents();

            InitializeMembers();
        }

        public ObservableCollection<DocumentWrapper> Documents { get; set; }

        public ObservableCollection<string> DocumentLoadExceptions { get; set; }

        public string CurrentStatus
        {
            get { return currentStatus; }
            set
            {
                currentStatus = value;
                OnPropertyChanged();
            }
        }

        private SentenceWrapper sentence;
        public SentenceWrapper SelectedSentence {
            get { return sentence; }
            set { sentence = value; OnPropertyChanged(); }
        }

        public DocumentWrapper SelectedDocument
        {
            get { return selectedDocument; }
            set
            {
                selectedDocument = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SentenceEditorView> SentenceEditViewModels
        {
            get { return sentenceEditViewModels; }
            set
            {
                sentenceEditViewModels = value;
                OnPropertyChanged();
            }
        }

        public ICommand NewTreeBankCommand { get; set; }

        public ICommand OpenCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand SaveAsCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        public ICommand EditSentenceCommand { get; set; }

        private void InitializeMembers()
        {
            documentsWrappers = new Dictionary<string, DocumentWrapper>();
            DocumentLoadExceptions = new ObservableCollection<string>();
            Documents = new ObservableCollection<DocumentWrapper>();
            sentenceEditViewModels = new ObservableCollection<SentenceEditorView>();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<DocumentLoadExceptionEvent>().Subscribe(OnDocumentLoadException);
            eventAggregator.GetEvent<StatusNotificationEvent>().Subscribe(OnStatusNotification);
        }

        private void OnStatusNotification(string statusNotification)
        {
            CurrentStatus = statusNotification;
        }

        private void OnDocumentLoadException(string exceptionMessage)
        {
            DocumentLoadExceptions.Add(exceptionMessage);
        }

        private void InitializeServices(
            IEventAggregator eventAggregatorArg,
            ISaveDialogService saveDialogServiceArg,
            IOpenFileDialogService openFileDialogServiceArg,
            IDocumentMapper documentMapperArg)
        {
            if (eventAggregatorArg == null)
            {
                throw new ArgumentNullException("eventAggregatorArg");
            }

            if (saveDialogServiceArg == null)
            {
                throw new ArgumentNullException("saveDialogServiceArg");
            }

            if (openFileDialogServiceArg == null)
            {
                throw new ArgumentNullException("openFileDialogServiceArg");
            }

            if (documentMapperArg == null)
            {
                throw new ArgumentNullException("documentMapperArg");
            }

            saveDialogService = saveDialogServiceArg;
            openFileDialogService = openFileDialogServiceArg;
            eventAggregator = eventAggregatorArg;
            documentMapper = documentMapperArg;
        }

        private void InitializeCommands()
        {
            NewTreeBankCommand = new DelegateCommand(NewTreeBankCommandExecute, NewTreeBankCommandCanExecute);
            OpenCommand = new DelegateCommand(OpenCommandExecute, OpenCommandCanExecute);
            SaveCommand = new DelegateCommand(SaveCommandExecute, SaveCommandCanExecute);
            SaveAsCommand = new DelegateCommand(SaveAsCommandExecute, SaveAsCommandCanExecute);
            CloseCommand = new DelegateCommand(CloseCommandExecute, CloseCommandCanExecute);
            EditSentenceCommand = new DelegateCommand(EditSentenceCommandExecute, EditSentenceCommandCanExecute);
        }

        private void EditSentenceCommandExecute(object obj)
        {
            SentenceEditViewModels.Add(new SentenceEditorView(new SentenceEditorViewModel(eventAggregator, SelectedSentence)));
            eventAggregator.GetEvent<StatusNotificationEvent>().Publish(string.Format("Editing sentence with ID: {0}, document ID: {1}", SelectedSentence.Id, SelectedDocument.Identifier));
        }

        private bool EditSentenceCommandCanExecute(object arg)
        {
            if (SelectedSentence != null)
            {
                return true;
            }

            return false;
        }

        private void InvalidateCommands()
        {
            ((DelegateCommand) NewTreeBankCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) OpenCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) SaveCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) SaveAsCommand).RaiseCanExecuteChanged();
            ((DelegateCommand) CloseCommand).RaiseCanExecuteChanged();
        }

        private bool CloseCommandCanExecute(object arg)
        {
            return SelectedDocument != null;
        }

        private void CloseCommandExecute(object obj)
        {
            if (SelectedDocument == null)
            {
                return;
            }

            var closedDocumentFilepath = SelectedDocument.Model.FilePath;

            documentsWrappers.Remove(closedDocumentFilepath);
            SelectedDocument = null;

            if (documentsWrappers.Any())
            {
                SelectedDocument = documentsWrappers.First().Value;
            }

            RefreshDocumentsExplorerList();
            InvalidateCommands();

            eventAggregator.GetEvent<StatusNotificationEvent>().Publish(string.Format("Document closed: {0}", closedDocumentFilepath));
        }

        public void OnClosing(CancelEventArgs cancelEventArgs)
        {
            // todo: show message box to confirm closing if there are changes in the viewmodel

            // todo:check if there are any unsaved changes, show popup to allow the user to decide, handle his response and then exit the app
        }

        private bool NewTreeBankCommandCanExecute(object arg)
        {
            return true;
        }

        private void NewTreeBankCommandExecute(object obj)
        {
            Documents.Add(new DocumentWrapper(new Document
            {
                Identifier = "Treebank" + Documents.Count
            }));

            eventAggregator.GetEvent<StatusNotificationEvent>().Publish("Treebank created");
        }

        private bool OpenCommandCanExecute(object arg)
        {
            return true;
        }

        private async void OpenCommandExecute(object obj)
        {
            var documentFilePath = openFileDialogService.GetFileLocation(FileFilters.XmlFilesOnlyFilter);

            eventAggregator.GetEvent<StatusNotificationEvent>().Publish(string.Format("Loading document: {0}", documentFilePath));

            if (string.IsNullOrWhiteSpace(documentFilePath))
            {
                return;
            }

            DocumentLoadExceptions.Clear();

            var documentModel = await documentMapper.Map(documentFilePath);

            //must check if the file is alredy loaded and has changes offer to save if so

            documentsWrappers[documentFilePath] = new DocumentWrapper(documentModel);

            RefreshDocumentsExplorerList();
            InvalidateCommands();
            eventAggregator.GetEvent<StatusNotificationEvent>().Publish(string.Format("Document loaded: {0}", documentFilePath));
        }

        private void RefreshDocumentsExplorerList()
        {
            Documents.Clear();

            foreach (var documentWrapper in documentsWrappers)
            {
                Documents.Add(documentWrapper.Value);
            }
        }

        private void SaveCommandExecute(object obj)
        {
            var documentFilePath = selectedDocument != null
                ? selectedDocument.Model.FilePath
                : saveDialogService.GetSaveFileLocation(FileFilters.XmlFilesOnlyFilter);

            eventAggregator.GetEvent<StatusNotificationEvent>().Publish("Saving document");

            if (string.IsNullOrWhiteSpace(documentFilePath))
            {
            }

            // todo: save logic

            eventAggregator.GetEvent<StatusNotificationEvent>().Publish(string.Format("Document saved: {0}", documentFilePath));
        }

        private bool SaveCommandCanExecute(object arg)
        {
            return true;
        }

        private bool SaveAsCommandCanExecute(object arg)
        {
            return true;
        }

        private void SaveAsCommandExecute(object obj)
        {
            eventAggregator.GetEvent<StatusNotificationEvent>().Publish("Saving document");

            var documentFilePath = saveDialogService.GetSaveFileLocation(FileFilters.AllFilesFilter);

            if (string.IsNullOrWhiteSpace(documentFilePath))
            {
            }

            // todo: save as logic
            eventAggregator.GetEvent<StatusNotificationEvent>().Publish(string.Format("Document saved: {0}", documentFilePath));
        }
    }
}