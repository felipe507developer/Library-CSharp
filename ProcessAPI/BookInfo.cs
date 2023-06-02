namespace Proceso{
    class BookInfo
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Authors { get; set; }
        public int NumberOfPages { get; set; }
        public string PublishDate { get; set; }

        public BookInfo(string title, string subtitle, string authors, int numberOfPages, string publishDate)
        {
            Title = title;
            Subtitle = subtitle;
            Authors = authors;
            NumberOfPages = numberOfPages;
            PublishDate = publishDate;
        }


    }
}