using Inventory.Application.DTOs.Customers;
using Inventory.Application.Interfaces;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;

namespace Inventory.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.CustomerRepository.ExistsAsync(w => w.Id == id, cancellationToken);
            return Result<bool>.Success(exists);
        }
        public async Task<Result<IEnumerable<CustomerDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var customers = await _unitOfWork.CustomerRepository.GetAllAsync(cancellationToken);
            var customerDtos = customers.Select(MapToDto);
            return Result<IEnumerable<CustomerDto>>.Success(customerDtos);
        }

        public async Task<Result<CustomerDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(id, cancellationToken);
            if (customer == null)
            {
                return Result<CustomerDto>.Failure($"Customer with ID {id} not found.");
            }
            var customerDto = MapToDto(customer);

            return Result<CustomerDto>.Success(customerDto);
        }

        public async Task<Result<CustomerDto>> CreateAsync(CreateCustomerDto createDto, CancellationToken cancellationToken = default)
        {
            var codes = await _unitOfWork.CustomerRepository.GetAllCustomerCodeAsync(cancellationToken);
            string generatedCode = GenerateCustomerCode(codes);

            // Code Empty Check
            if (string.IsNullOrWhiteSpace(generatedCode))
                return Result<CustomerDto>.Failure("Failed to generate a valid Customer code.");

            // Code Exists Check
            if (await _unitOfWork.CustomerRepository.ExistsByCodeAsync(generatedCode, cancellationToken))
                return Result<CustomerDto>.Failure($"Customer with code '{generatedCode}' already exists.");

            var customer = MapToEntity(createDto, generatedCode);
            await _unitOfWork.CustomerRepository.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var customerDto = MapToDto(customer);
            return Result<CustomerDto>.Success(customerDto);
        }

        public async Task<Result<CustomerDto>> UpdateAsync(int id, UpdateCustomerDto updateDto, CancellationToken cancellationToken = default)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(id, cancellationToken);

            if (customer == null)
            {
                return Result<CustomerDto>.Failure($"Customer with ID {id} not found.");
            }

            if (updateDto.RowVersion != null && customer.RowVersion != null)
            {
                customer.RowVersion = updateDto.RowVersion;
            }

            // Check if customer code is being changed and if new code already exists
            if (customer.CustomerCode != updateDto.CustomerCode)
            {
                var existingWarehouse = await _unitOfWork.WarehouseRepository.GetByCodeAsync(updateDto.CustomerCode, cancellationToken);
                if (existingWarehouse != null && existingWarehouse.Id != id)
                {
                    return Result<CustomerDto>.Failure($"Customer with code '{updateDto.CustomerCode}' already exists.");
                }
            }

            MapToEntity(updateDto, customer);
            await _unitOfWork.CustomerRepository.UpdateAsync(customer, cancellationToken);
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<CustomerDto>.Failure("Customer đã được cập nhật bởi người dùng khác. Vui lòng tải lại dữ liệu và thử lại.");
            }

            var customerDto = MapToDto(customer);
            return Result<CustomerDto>.Success(customerDto);
        }
        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(id, cancellationToken);

            if (customer == null)
            {
                return Result.Failure($"Customer with ID {id} not found.");
            }

            // Soft delete
            customer.IsDeleted = true;
            await _unitOfWork.CustomerRepository.UpdateAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        #region Helpers

        private static Customer MapToEntity(CreateCustomerDto createDto, string customerCode)
        {
            return new Customer
            {
                CustomerCode = customerCode,
                CustomerName = createDto.CustomerName,
                Address = createDto.Address,
                PhoneNumber = createDto.PhoneNumber,
                IsActive = createDto.IsActive,
                Description = createDto.Description
            };
        }

        private static void MapToEntity(UpdateCustomerDto updateDto, Customer customer)
        {
            customer.CustomerCode = updateDto.CustomerCode;
            customer.CustomerName = updateDto.CustomerName;
            customer.Address = updateDto.Address;
            customer.PhoneNumber = updateDto.PhoneNumber;
            customer.IsActive = updateDto.IsActive;
            customer.Description = updateDto.Description;
        }

        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                CustomerCode = customer.CustomerCode,
                CustomerName = customer.CustomerName,
                Address = customer.Address,
                PhoneNumber = customer.PhoneNumber,
                IsActive = customer.IsActive,
                Description = customer.Description,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                RowVersion = customer.RowVersion
            };
        }

        private string GenerateCustomerCode(List<string> codes)
        {
            if (codes == null || !codes.Any())
                return "CUS - 001";

            int maxNumber = codes
                .Select(x => x.Split('-')[1].Trim())
                .Select(x => int.Parse(x))
                .Max();

            int nextNumber = maxNumber + 1;
            return $"CUS - {nextNumber:D3}";
        }

        #endregion
    }
}


