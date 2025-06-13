using System;

namespace W2.Application.Contracts.IMS;
public class InputToUpdateUserStatusDto
{
    public string EmailAddress { get; set; }
    public DateTime DateAt { get; set; }
}