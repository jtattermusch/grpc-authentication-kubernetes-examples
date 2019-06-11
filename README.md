# Authentication and Security in gRPC Microservices Examples

A collection of simple examples to accompany the "Authentication and Security in gRPC Microservices Examples"
talk.

https://kccnceu19.sched.com/event/MPbC

Full talk video: https://www.youtube.com/watch?v=_y-lzjdVEf0


## Prework

This examples have been setup on Google Kubernetes Engines, but they should work on any other kubernetes cluster (public or private).

1. Create a demo GKE cluster https://cloud.google.com/kubernetes-engine/docs/how-to/creating-a-container-cluster
   and set up the `gcloud` to make it the default cluster.
2. Make sure you can use the `kubectl` command line tool by following: https://cloud.google.com/kubernetes-engine/docs/quickstart

## Build the docker images

Build the docker images and push them to container registry so that we can later deploy them in
our kubernetes cluster.

```
$ kubernetes/docker_build_and_push.sh
```


## Example 1: Connect securely using TLS

```
# deploy client and server
$ kubectl create -f greeter-server-tls.yaml
$ kubectl create -f greeter-client-tls.yaml

# show client logs
$ kubectl logs greeter-client-tls
```

## Example 2: Authenticate request with a JWT token

Assumes server from previous example is still running

```
$ kubectl create -f greeter-client-jwt.yaml

# show client logs indicating that we've authenticated using JWT token
$ kubectl logs greeter-client-jwt
```

## Example 3: Mutual TLS with manually provided certificates

```
# replace existing server with one that requires mutual authentication
$ kubectl apply -f greeter-server-mtls.yaml

# deploy client with mTLS
$ kubectl create -f greeter-client-mtls.yaml

# show client logs indicating that we've authenticated using mTLS
$ kubectl logs greeter-client-mtls
```

## Example 4: Mutual TLS with istio and automated key rotation 

gRPC client and server are using insecure channels and trust the proxy to perform mutual authentication on their behalf.

Before running the examples, you must install istio using instuctions in https://istio.io/docs/setup/kubernetes/install/kubernetes/
and running `kubectl apply -f install/kubernetes/istio-demo-auth.yaml`

```
$ kubectl apply -f <(istioctl kube-inject -f greeter-server-istio.yaml)
$ kubectl create -f <(istioctl kube-inject -f greeter-client-istio.yaml)
```

```
# show that client can make requests to server
$ kubectl logs greeter-client-istio greeter-client-istio

# show that mTLS is actually used by the service mesh 
$ kubectl logs greeter-client-istio greeter-client-istio
```

## Contents

- `kubernetes`: configuration for running examples on Kubernetes
