using Microsoft.EntityFrameworkCore;
using Mawidy.Domain.Entities;

namespace Mawidy.Application.Interfaces;

/// <summary>
/// Unified database context interface exposing all DbSets for the application.
/// Replaces the former IAppDbContext and IApplicationDbContext split.
/// </summary>
public interface IApplicationDbContext
{
    // ── Civil Registry / Telecom ─────────────────────────────────────────
    DbSet<Operator>          Operators           { get; }
    DbSet<Branch>            Branches            { get; }
    DbSet<Governorate>       Governorates        { get; }
    DbSet<District>          Districts           { get; }
    DbSet<OperatorService>   OperatorServices    { get; }
    DbSet<ServiceDocument>   ServiceDocuments    { get; }
    DbSet<Appointment>       Appointments        { get; }
    DbSet<VirtualQueueEntry> VirtualQueueEntries { get; }

    // ── Courts ──────────────────────────────────────────────────────────
    DbSet<Court> Courts { get; }
    DbSet<CourtDepartment> CourtDepartments { get; }
    DbSet<CourtService> CourtServices { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<LegalCase> LegalCases { get; }
    DbSet<CaseTimelineEvent> CaseTimelineEvents { get; }
    DbSet<QueueTicket> QueueTickets { get; }

    // ── Banks ───────────────────────────────────────────────────────────
    DbSet<Mawidy.Domain.Entities.Banks.Service> BankServices { get; }

    // ── Hospitals ────────────────────────────────────────────────────────
    DbSet<Mawidy.Domain.Entities.Hospitals.Hospitals> Hospitals { get; }
    DbSet<Mawidy.Domain.Entities.Hospitals.Beds> HospitalBeds { get; }
    DbSet<Mawidy.Domain.Entities.Hospitals.BedTypes> HospitalBedTypes { get; }
    DbSet<Mawidy.Domain.Entities.Hospitals.Reservations> HospitalReservations { get; }
    DbSet<Mawidy.Domain.Entities.Hospitals.BlockedPhones> HospitalBlockedPhones { get; }
    DbSet<Mawidy.Domain.Entities.Hospitals.Reports> HospitalReports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
