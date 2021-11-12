using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Hubs
{
	public record PerformCancelAndDeleteThisWorkTimerParams(
		string? SharedSecret,
		Guid? CompanyId,
		string? CompanyPhoneId,
		Guid? LabourId,
		Guid? AgentId,
		string? AgentPhoneId,
		string? EnteredPasscode
		);
}