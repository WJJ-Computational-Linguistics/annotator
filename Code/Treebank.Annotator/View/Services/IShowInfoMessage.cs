﻿namespace Treebank.Annotator.View.Services
{
    using System.Windows;

    public interface IShowInfoMessage
    {
        MessageBoxResult ShowInfoMessage(string message, MessageBoxButton buttons);
    }
}