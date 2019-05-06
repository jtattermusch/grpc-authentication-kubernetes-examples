# Authentication and Security in gRPC Microservices Examples

A collection of simple examples to accompany the "Authentication and Security in gRPC Microservices Examples"
talk.

https://kccnceu19.sched.com/event/MPbC

Talk Slides: TBD

Full talk video: TBD


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

## Example 1: ...


## Contents

- `kubernetes`: configuration for running examples on Kubernetes
