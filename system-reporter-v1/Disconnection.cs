using hardware_connetion_monitor;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace system_reporter_v1;

public record Disconnection(string HardwareUnitId, HardwareConnectionState State, DateTime StartTime)
{
    [JsonIgnore]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; init; } = string.Empty;

    public DateTime? EndTime { get; set; }
}
