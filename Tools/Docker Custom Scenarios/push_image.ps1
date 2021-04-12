# registry credentials for QA
$registryUrl = ""
$registryUsername = ""
$registryPassword = "+m4Oh6dIu0Q"

# image name. update as appropriate
$imagename = ""

docker-compose build

docker login -u $registryUsername -p $registryPassword  $registryUrl
docker tag $imagename $registryUrl/$imagename
docker push $registryUrl/$imagename