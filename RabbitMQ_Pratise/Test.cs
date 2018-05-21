using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ_Pratise
{
    [TestClass]
    public class Test
    {
        private ConnectionFactory _factory = new ConnectionFactory() { HostName = "localhost", UserName = "test", Password = "123123" };

        [TestMethod]
        public void BasicPublish()
        {
            using (var connection = this._factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("hello", false, false, false, null);
                    var body = Encoding.UTF8.GetBytes("Hello World");
                    channel.BasicPublish("", "hello", null, body);
                }
            }
        }

        [TestMethod]
        public void Receive()
        {
            using (var connection = this._factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("hello", false, false, false, null);

                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("hello", true, consumer);

                    while (true)
                    {
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                        var message = Encoding.UTF8.GetString(ea.Body);
                    }
                }
            }
        }
    }
}
