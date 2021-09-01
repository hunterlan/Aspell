namespace Logic
{
    public class CheckerFactory
    {
        public static IChecker GetCheckerObject()
        {
            return new Checker();
        }
    }
}