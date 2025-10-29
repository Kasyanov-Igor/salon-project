using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Record
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!; // Название

        public string NameMaster { get; set; } = null!;

        public int Places { get; set; }

        public string? ClientName { get; set; }

        public string? Description { get; set; } // Описание

        public int DurationMinutes { get; set; } // Предполагаемая продолжительность

        public DateTime BookingTime { get; set; } // Время создания записи

        public int MasterId { get; set; } // Внешний ключ к мастеру
    }
}
