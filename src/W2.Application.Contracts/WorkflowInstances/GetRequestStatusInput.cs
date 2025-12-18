using System;
using Volo.Abp.Application.Dtos;

namespace W2.WorkflowInstances
{
    public class GetRequestStatusInput : PagedAndSortedResultRequestDto
    {
        public string Email { get; set; }
        public DateTime? Date { get; set; }
    }
}