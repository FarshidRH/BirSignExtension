namespace MapIdeaHub.BirSign.NetFrameworkExtension.Dtos
{
    public class ApiReponseDto<T>
    {
        public T Data { get; set; }

        public bool IsSuccess { get; set; } = true;

        public string Error { get; set; }
    }
}
