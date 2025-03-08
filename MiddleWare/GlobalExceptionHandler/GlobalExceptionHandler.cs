using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using NLog;
using MiddleWare.GlobalExceptionHandler;
using MiddleWare.GlobalExceptionHandler;
using System.Threading;

namespace MiddleWare.GlobalExceptionHandler
{
    public class GlobalExceptionHandler : ExceptionFilterAttribute
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override void OnException(ExceptionContext context)
        {
            var errorResponse = ExceptionHandler.CreateErrorResponse(context.Exception);

            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = 500
            };

            context.ExceptionHandled = true;
        }



    }
}