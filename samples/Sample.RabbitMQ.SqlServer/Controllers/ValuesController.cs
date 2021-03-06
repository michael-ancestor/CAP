﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;

namespace Sample.RabbitMQ.SqlServer.Controllers
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return "Name:" + Name + ";Age:" + Age;
        }
    }


    [Route("api/[controller]")]
    public class ValuesController : Controller, ICapSubscribe
    {
        private readonly ICapPublisher _capBus;
        private readonly AppDbContext _dbContext;

        public ValuesController(ICapPublisher producer, AppDbContext dbContext)
        {
            _capBus = producer;
            _dbContext = dbContext;
        }

        [Route("~/publish")]
        public IActionResult PublishMessage()
        {
            using(var trans = _dbContext.Database.BeginTransaction())
            {
                //_capBus.Publish("sample.rabbitmq.mysql22222", DateTime.Now);
                _capBus.Publish("sample.rabbitmq.mysql33333", new Person { Name = "宜兴", Age = 11 });
                trans.Commit();
            }
            return Ok();
        }

        [Route("~/publishWithTrans")]
        public async Task<IActionResult> PublishMessageWithTransaction()
        {
            using (var trans = await _dbContext.Database.BeginTransactionAsync())
            {
                await _capBus.PublishAsync("sample.rabbitmq.mysql", "");
                 
                trans.Commit();
            }
            return Ok();
        }

        [CapSubscribe("sample.rabbitmq.mysql33333")]
        public void KafkaTest22(Person person)
        {
            var aa = _dbContext.Database;

            _dbContext.Dispose();

            Console.WriteLine("[sample.kafka.sqlserver] message received   " + person.ToString());
            Debug.WriteLine("[sample.kafka.sqlserver] message received   " + person.ToString());
        }

        //[CapSubscribe("sample.rabbitmq.mysql22222")]
        //public void KafkaTest22(DateTime time)
        //{
        //    Console.WriteLine("[sample.kafka.sqlserver] message received   " + time.ToString());
        //    Debug.WriteLine("[sample.kafka.sqlserver] message received   " + time.ToString());
        //}

        [CapSubscribe("sample.rabbitmq.mysql22222")]
        public async Task<DateTime> KafkaTest33(DateTime time)
        {
            Console.WriteLine("[sample.kafka.sqlserver] message received   " + time.ToString());
            Debug.WriteLine("[sample.kafka.sqlserver] message received   " + time.ToString());
            return await Task.FromResult(time);
        }

        [NonAction]
        [CapSubscribe("sample.kafka.sqlserver3")]
        [CapSubscribe("sample.kafka.sqlserver4")]
        public void KafkaTest()
        {
            Console.WriteLine("[sample.kafka.sqlserver] message received");
            Debug.WriteLine("[sample.kafka.sqlserver] message received");
        }
    }
}