﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Databases.Records
{
	public record Address(Guid? id, string Label, string Value);
}
