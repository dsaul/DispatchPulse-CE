using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Hubs
{
	public record PerformSaveThisWorkTimerAndCompleteTheAssignmentResponse(bool IsError, string? ErrorMessage, bool Completed);
}
