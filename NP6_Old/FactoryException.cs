////
/// Copyright (c) 2016 Saúl Piña <sauljabin@gmail.com>.
/// 
/// This file is part of xmlrpcwsc.
/// 
/// xmlrpcwsc is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Lesser General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
/// 
/// xmlrpcwsc is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Lesser General Public License for more details.
/// 
/// You should have received a copy of the GNU Lesser General Public License
/// along with xmlrpcwsc.  If not, see <http://www.gnu.org/licenses/>.
////

using System;

namespace RPC_Service.NP6
{

    /// <summary>
    /// Factory exception
    /// </summary>
    public class FactoryException : Exception {

        public FactoryException() {
        }

        public FactoryException(string message) : base(message) {
        }

        public FactoryException(string message, Exception inner) : base(message, inner) {
        }
    }
}

