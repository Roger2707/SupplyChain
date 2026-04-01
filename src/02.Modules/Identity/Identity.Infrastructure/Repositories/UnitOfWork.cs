using Identity.Application.Interfaces;
using Identity.Infrastructure.Data;
using SharedKernel.Repositories;

namespace Identity.Infrastructure.Repositories
{
    public class UnitOfWork : EfUnitOfWorkBase<IdentityDbContext>, IUnitOfWork
    {
        private readonly IdentityDbContext _context;
        private readonly Dictionary<Type, object> _repositories;

        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IPermissionRepository _permissionRepository;

        public UnitOfWork(IdentityDbContext context)
            : base(context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);

            if (_repositories.ContainsKey(type))
            {
                return (IRepository<T>)_repositories[type];
            }

            var repository = new Repository<T>(_context);
            _repositories[type] = repository;
            return repository;
        }

        public IUserRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(_context);
                }
                return _userRepository;
            }
        }

        public IRoleRepository RoleRepository
        {
            get
            {
                if (_roleRepository == null)
                {
                    _roleRepository = new RoleRepository(_context);
                }
                return _roleRepository;
            }
        }

        public IPermissionRepository PermissionRepository
        {
            get
            {
                if (_permissionRepository == null)
                {
                    _permissionRepository = new PermissionRepository(_context);
                }
                return _permissionRepository;
            }
        }
    }
}



