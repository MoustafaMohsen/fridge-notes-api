using FridgeServer.Helpers;
using MLiberary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.Models.Dto
{
    public class ResponseDto
    {
        public object value { get; set; }
        public string statusText { get; set; }
        public string errors { get; set; } = null;
        public bool isSuccessful => M.isNull(errors) ? true : false;
    }
    public class ResponseDto<T> : ResponseDto
    {
        public new T value { get=>(T)base.value; set=>base.value = value; }
    }
}
