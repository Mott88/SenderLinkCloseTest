using Amqp;
using Amqp.Framing;
using System.Text;

namespace SenderLinkCloseTest
{
    internal class Program
    {
        private static readonly string _username = "guest";
        private static readonly string _password = "guest";
        private static readonly string _host = "localhost";
        private static readonly string _port = "5672";

        private static readonly Address _address = new($"amqp://{_username}:{Uri.EscapeDataString(_password)}@{_host}:{_port}");

        // AMQP heartbeat equals the IdleTimeOut divided by 2. In this case 30 seconds.
        private static readonly Open open = new() { IdleTimeOut = 60000 };

        static void Main(string[] args)
        {
            Connection connection = new(_address, null, open, null);
            Session session1 = new(connection);

            Target target1 = new()
            {
                Durable = 1,
                Address = "/amq/queue/queue1"
            };

            SenderLink sender = new(session1, "test", target1, null);

            string largeString = new string('*', 100000000);

            Message outgoingMessage = new()
            {
                BodySection = new Data()
                {
                    Binary = Encoding.UTF8.GetBytes(largeString)
                },
                Properties = new Properties()
                {
                    MessageId = "a-b-c",
                    To = "1234",
                    Subject = "TEST_MESSAGE"
                },
                Header = new Header()
                {
                    Durable = true,
                    Ttl = 90000
                },
                ApplicationProperties = new ApplicationProperties()
                {
                    ["fromHerID"] = "1234",
                    ["toHerID"] = "5678"
                }
            };

            sender.SendAsync(outgoingMessage);
            sender.Cancel(outgoingMessage);

            Console.ReadKey();
        }
    }
}
