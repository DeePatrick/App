using App.Models;

namespace App.Repositories
{
    public interface ICompanyRepository
    {
        Company GetById(int id);
    }
}