using System;

namespace App.Services
{
    public class DateTimeProvider: IDateTimeProvider
    {
        public DateTime DateTimeNow => DateTime.Now;
    }
}