﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Recurly
{
    /// <summary>
    /// The requested object is not defined in Recurly.
    /// </summary>
    public class NotFoundException : RecurlyException
    {
        internal NotFoundException(string message, RecurlyError[] errors)
            : base(message, errors)
        { }
    }
}
