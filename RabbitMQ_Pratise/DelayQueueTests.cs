using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ_Pratise
{
    [TestClass]
    public class DelayQueueTests
    {
        private ConnectionFactory _factory = new ConnectionFactory() { HostName = "localhost", UserName = "test", Password = "123123" };

        [TestMethod]
        public void BasicPublish()
        {
            using (var connection = this._factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var dic = new Dictionary<string, object>
                    {
                        ["x-expires"] = 30000,
                        ["x-message-ttl"] = 12000, // 队列上消息过期时间，应小于队列过期时间  
                        ["x-dead-letter-exchange"] = "exchange-direct", // 过期消息转向路由  
                        ["x-dead-letter-routing-key"] = "routing-delay", // 过期消息转向路由相匹配routingkey  
                    };
                    channel.QueueDeclare("hello", true, false, false, dic);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

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
                    var name = channel.QueueDeclare().QueueName;
                    channel.ExchangeDeclare("exchange-direct", "direct");
                    channel.QueueBind(name, "exchange-direct", "routing-delay");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        channel.BasicAck(ea.DeliveryTag, false);
                    };
                    channel.BasicConsume(name, true, consumer);
                }
            }
        }
    }
}
