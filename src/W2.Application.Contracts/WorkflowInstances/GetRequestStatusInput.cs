using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace W2.WorkflowInstances
{
    public class GetRequestStatusInput : PagedAndSortedResultRequestDto
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string MezonId { get; set; }
        
        [Required]
        public DateTime? Date { get; set; }
    }
}