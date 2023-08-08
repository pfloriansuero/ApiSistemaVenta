using AutoMapper;
using Azure;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SistemaVenta.BLL.Servicios
{
    public class UsuarioService:IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _usuarioRepository;
        private readonly IMapper _mapper;

        public UsuarioService(IGenericRepository<Usuario> usuarioRepository, IMapper mapper)
        {
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
        }

        public async Task<List<UsuarioDTO>> Lista()
        {
            try
            {   
                var queryUsuarios = await _usuarioRepository.Consultar();
                var listaUsuarios = queryUsuarios.Include(rol => rol.IdRolNavigation).ToList();
                return _mapper.Map<List<UsuarioDTO>>(listaUsuarios.ToList());
            }
            catch 
            {

                throw;
            }
        }

        public async Task<SesionDTO> ValidarCredenciales(string correo, string clave)
        {
            try
            {
                var queryUsuario = await _usuarioRepository.Consultar(u => 
                u.Correo == correo &&
                u.Clave == clave);

                //if (queryUsuario.FirstOrDefault() == null)
                //{
                //    throw new TaskCanceledException("El usuario no existe");
                //}
                //else
                //{
                //    Usuario devolverUsuario = queryUsuario.Include(rol => rol.IdRolNavigation).First();
                //}

                if (queryUsuario.FirstOrDefault() == null)
                    throw new TaskCanceledException("El usuario no existe");

                Usuario devolverUsuario = queryUsuario.Include(rol => rol.IdRolNavigation).First();

                return _mapper.Map<SesionDTO>(devolverUsuario);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async  Task<UsuarioDTO> Crear(UsuarioDTO modelo)
        {
          
            try
            {
                
                var validacion = _mapper.Map<Usuario>(modelo);



                var usuarioExistente = await _usuarioRepository.Consultar(u => u.Correo == validacion.Correo);
                if (usuarioExistente.FirstOrDefault() != null)
                    throw new Exception("Ya existe un usuario con ese correo");

                //return "Ya existe un usuario registrado con el mismo correo electrónico";

                var usuarioCreado = await _usuarioRepository.Crear(_mapper.Map<Usuario>(modelo));
                if (usuarioCreado.IdUsuario == 0)
                    throw new TaskCanceledException("No se pudo crear el usuario");



                var query = await _usuarioRepository.Consultar(u => u.IdUsuario == usuarioCreado.IdUsuario);

                usuarioCreado = query.Include(rol => rol.IdRolNavigation).First();

                return _mapper.Map<UsuarioDTO>(usuarioCreado);
            }
            catch 
            {

                throw;
            }
        }

        public async Task<bool> Editar(UsuarioDTO modelo)
        {
            try
            {
                var usuarioModelo = _mapper.Map<Usuario>(modelo);
                var usuarioEncontrado = await _usuarioRepository.Obtener(u => u.IdUsuario == usuarioModelo.IdUsuario);

                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("Este usuario no existe");

                usuarioEncontrado.NombreCompleto = usuarioModelo.NombreCompleto;
                usuarioEncontrado.Correo = usuarioModelo.Correo;
                usuarioEncontrado.Clave = usuarioModelo.Clave;
                usuarioEncontrado.IdRol = usuarioModelo.IdRol;
                usuarioEncontrado.EsActivo = usuarioModelo.EsActivo;

                bool respuesta = await _usuarioRepository.Editar(usuarioEncontrado);

                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar el usuario");

                return respuesta;
            }
            catch 
            {

                throw;
            }

        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepository.Obtener(u  => u.IdUsuario == id);

                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("Este usuario no existe");

                bool respuesta = await _usuarioRepository.Eliminar(usuarioEncontrado);

                if (!respuesta) 
                    throw new TaskCanceledException("No se pudo eliminar el usuario");

                return respuesta;
            }
            catch 
            {

                throw;
            }
        }

       
    }
}
