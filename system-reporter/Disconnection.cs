using hardware_connetion_monitor;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace system_reporter;

public record Disconnection(string HardwareUnitId, HardwareConnectionState State, DateTime StartTime)
{
    [JsonIgnore]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; init; }

    public DateTime? EndTime { get; set; }
}
