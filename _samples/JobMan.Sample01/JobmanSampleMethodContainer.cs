namespace JobMan.Sample01
{
    public class JobmanSampleMethodContainer
    {
        public static void Job1(string myParam1, int myParam2)
        {
            //Do something

            Random random = new Random(DateTime.Now.Millisecond);
            int randomMSec1 = random.Next(1, 100);

            int longTaskDice = random.Next(1, 10);
            if (longTaskDice > 8)
                randomMSec1 = randomMSec1 * 10;

            System.Threading.Thread.Sleep(randomMSec1);
        }
    }
}
