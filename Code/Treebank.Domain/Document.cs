﻿namespace Treebank.Domain
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Document : Element
    {
        public Document()
        {
            Sentences = new List<Sentence>();
        }

        public ICollection<Sentence> Sentences { get; set; }

        public string FilePath { get; set; }
    }
}