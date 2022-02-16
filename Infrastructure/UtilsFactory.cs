namespace Infrastructure
{
    public class UtilsFactory
    {
        public static IUtils GetUtilsObject()
        {
            return new Utils();
        }
    }   
}