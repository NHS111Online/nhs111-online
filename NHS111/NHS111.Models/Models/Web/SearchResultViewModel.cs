﻿namespace NHS111.Models.Models.Web {
    using System.Collections.Generic;

    public class SearchResultViewModel {
        public string PathwayTitle { get; set; }
        public List<string> Title { get; set; }
        public List<string> HighlightedTitle { get; set; }
        public object TitlePhonetic { get; set; }
        public string Description { get; set; }
        public object DescriptionPhonetic { get; set; }
        public string PathwayNo { get; set; }
        public List<string> Gender { get; set; }
        public List<string> AgeGroup { get; set; }
        public object Score { get; set; }
    }
}