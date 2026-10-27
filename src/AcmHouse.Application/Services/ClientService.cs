using AcmHouse.Application.Abstractions;
using AcmHouse.Application.Dtos;
using AcmHouse.Application.Exceptions;
using AcmHouse.Domain.Entities;

namespace AcmHouse.Application.Services;

public class ClientService(IClientRepository clients, IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<ClientDto>> SearchAsync(string? search, CancellationToken ct = default)
    {
        string? text = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var found = await clients.SearchAsync(text, ct);
        return found.Select(c => c.ToDto()).ToList();
    }

    public async Task<ClientDto> GetAsync(int id, CancellationToken ct = default)
    {
        var client = await clients.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Client {id} was not found.");
        return client.ToDto();
    }

    public async Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        string passport = request.PassportNumber.Trim();
        if (await clients.PassportExistsAsync(passport, ct))
        {
            throw new ConflictException($"A client with passport {passport} already exists.");
        }

        var client = new Client
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            PassportNumber = passport
        };

        await clients.AddAsync(client, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return client.ToDto();
    }
}
