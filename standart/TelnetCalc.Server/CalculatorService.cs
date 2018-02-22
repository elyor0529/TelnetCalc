using System;
using System.Collections.Generic;
using System.Linq;
using Hik.Communication.ScsServices.Service;
using TelnetCalc.Common;

namespace TelnetCalc.Server
{
    public class CalculatorService : ScsService, ICalculatorService
    {
        private static readonly IDictionary<long,int> Numbers=new Dictionary<long, int>();

        private long UserId => CurrentClient.ClientId;

        public void Init()
        {
            Numbers.Add(UserId,0);
        }

        public int Add(int number)
        {

            lock (Numbers)
            {
                var prevSum = Numbers[UserId];
                var nextSum = prevSum+ number;

                Numbers[UserId] = nextSum;
               
                return nextSum;
            }
        }

        public void Clear()
        {
            lock (Numbers)
            {
                if (Numbers.ContainsKey(UserId))
                    Numbers.Remove(UserId); 
            }

        }
        
        public int Sum()
        {
            lock (Numbers)
            {
                return Numbers.Sum(a => a.Value);
            }
        }
         
    }
}