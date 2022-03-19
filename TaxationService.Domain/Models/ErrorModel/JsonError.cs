namespace TaxationService.Domain.Models.ErrorModel
{
    public sealed class JsonErrors
    {

        public JsonErrors()
        {

        }

        public List<JsonError> Errors
        {
            get;
            set;
        }
    }

    public class JsonError
    {
        public JsonError()
        {

        }

        public string Title
        {
            get;
            set;
        }

        public string Detail
        {
            get;
            set;
        }
        public string Status
        {
            get;
            set;
        }
        public string Code
        {
            get;
            set;
        }

        public static class StandardCodes
        {
            public const string ProductClassNotFoundError = "801";
            public const string ArgumentException = "812";
        }
    }
}
