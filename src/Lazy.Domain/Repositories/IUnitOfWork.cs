﻿using System.Data;

namespace Lazy.Domain.Repositories;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    IDbTransaction BeginTransaction();
}