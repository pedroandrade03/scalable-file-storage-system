using Bogus;
using Moq;
using StorageSystem.Application.Interfaces;

namespace StorageSystem.UnitTests.Common;

public abstract class BaseFixture
{
    public Faker Faker { get; } = new("pt_BR");

    public Mock<IUnitOfWork> GetUnitOfWorkMock() => new();

    public bool GetRandomBoolean() => Faker.Random.Bool();
}
