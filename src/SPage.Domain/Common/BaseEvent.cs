using System.ComponentModel.DataAnnotations.Schema;
using MediatR;

namespace SPage.Domain.Common;

[NotMapped]
public abstract class BaseEvent : INotification { }
