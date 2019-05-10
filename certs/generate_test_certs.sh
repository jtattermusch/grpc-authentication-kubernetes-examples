#!/bin/bash
# Generate CA and server and client certificate
# Modified version of https://github.com/grpc/grpc-java/tree/master/examples/example-tls

# Changes these CN's to match your hosts in your environment if needed.
SERVER_CA_CN=localhost-ca
SERVER_CN=localhost
CLIENT_CN=localhost # Used when doing mutual TLS

echo Generate CA:
openssl req -x509 -new -newkey rsa:4096 -keyout ca.key -nodes -out ca.pem -days 3650 -subj "/CN=${SERVER_CA_CN}"

echo Generate server key:
openssl genrsa -out server.key.rsa 4096
openssl pkcs8 -topk8 -in server.key.rsa -out server.key -nocrypt
rm server.key.rsa

echo Generate server certificate:
openssl req -new -key server.key -out server.csr -subj "/CN=${SERVER_CN}"
openssl x509 -req -in server.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out server.pem -days 365
rm server.csr

echo Generate client key:
openssl genrsa -out client.key.rsa 4096
openssl pkcs8 -topk8 -in client.key.rsa -out client.key -nocrypt
rm client.key.rsa

echo Generate client certificate:
openssl req -new -key client.key -out client.csr -subj "/CN=${CLIENT_CN}"
openssl x509 -req -in client.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out client.pem -days 365
rm client.csr
