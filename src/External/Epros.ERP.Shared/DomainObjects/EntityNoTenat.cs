using Flunt.Notifications;

namespace Epros.ERP.Shared.DomainObjects
{
    public abstract class EntityNoTenat : Notifiable<Notification>
    {
        public long Id { get; protected set; }
        public DateTime DataCadastro { get; protected set; }
        public DateTime? DataAlteracao { get; protected set; }
        public DateTime? Deletado { get; protected set; }

        protected EntityNoTenat()
        {
            DataCadastro = DateTime.Now;
            DataAlteracao = null;
            Deletado = null;
        }

        public virtual void Deletar()
        {
            DataAlteracao = DateTime.Now;
            Deletado = DateTime.Now;
        }

        public override bool Equals(object? obj)
        {
            var compareTo = obj as Entity;
            if (ReferenceEquals(this, compareTo)) return true;
            if (compareTo is null) return false;
            return Id.Equals(compareTo.Id);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        public override int GetHashCode() => GetType().GetHashCode() * 907 + Id.GetHashCode();

        public override string ToString() => $"{GetType().Name} [Id = {Id}]";
    }
}