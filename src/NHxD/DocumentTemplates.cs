using Nhentai;

namespace NHxD
{
	public class DocumentTemplates
	{
		public DocumentTemplate<object> About { get; set; }
		public DocumentTemplate<Metadata> Details { get; set; }
		public DocumentTemplate<Metadata> DetailsPreload { get; set; }
		public DocumentTemplate<Metadata> Download { get; set; }
		public DocumentTemplate<Metadata> GalleryTooltip { get; set; }
		public DocumentTemplate<ISearchProgressArg> LibraryCovergrid { get; set; }
		public DocumentTemplate<Metadata> LibraryCovergridItem { get; set; }
		public DocumentTemplate<ISearchProgressArg> SearchCovergrid { get; set; }
		public DocumentTemplate<Metadata> SearchCovergridItem { get; set; }
		public DocumentTemplate<ISearchArg> SearchPreload { get; set; }
		public DocumentTemplate<object> Startup { get; set; }

		public DocumentTemplates()
		{
		}
	}
}
