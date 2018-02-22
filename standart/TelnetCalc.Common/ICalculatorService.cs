using System;
using Hik.Communication.ScsServices.Service;

namespace TelnetCalc.Common
{
    [ScsService]
    public interface ICalculatorService
    {

        void Init();
        
        int Add(int number);
 
        void Clear();

        int  Sum();
        
    }
}