﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace API.Hubs
{
	public record PerformARIMarkAssignmentActiveWorkAsDoneParams(
		string? SharedSecret,
		Guid? CompanyId,
		string? CompanyPhoneId,
		Guid? AssignmentId,
		Guid? AgentId,
		string? AgentPhoneId,
		string? EnteredPasscode
		);
}
