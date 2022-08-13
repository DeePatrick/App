using System;

namespace App.Services
{
    public interface IDateTimeProvider
    {
        DateTime DateTimeNow { get;}
    }
}