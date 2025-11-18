namespace SourceGenerationMain.Inheritance;

    public class Foo
    {
        private string _message = "hello";

        protected bool WillWrite { get; set; }

        public void Bar()
        {
            Console.WriteLine(_message);
        }
    }
