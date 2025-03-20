﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiddleWare.RabbitMQClient
{
    public interface IRabbitMQService
    {
        void SendMessage(string message);
        void ReceiveMessage();
    }
}