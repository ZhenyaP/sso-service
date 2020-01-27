import groovy.transform.Field
import hudson.FilePath
import hudson.plugins.git.util.BuildData
import hudson.model.Run
import org.apache.commons.lang3.text.StrSubstitutor
import java.util.Map
import java.util.List
import groovy.json.JsonSlurperClassic

@Field final String SCMRelativeTargetDirectory = "-identity-provider"
@Field final String AwsKeysFileName = 'aws-keys.txt'
@Field final String AwsAccessKeyId = ''
@Field final String AwsSecretAccessKey = ''
@Field final String AwsSessionToken = ''
@Field final String DockerLoginAwsAccessKeyId = ''
@Field final String DockerLoginAwsSecretAccessKey = ''
@Field final String DockerLoginAwsSessionToken = ''
@Field final String AwsAccountId = "12345"
@Field final String AwsRegion = "us-east-1"

public class IdentityProviderConstants {
    public static final String CommonProjectName = "IdentityProvider.Common"
    public static final String ApiProjectName = "IdentityProvider.API"
    public static final String ApiTestProjectName = "IdentityProvider.API.Tests"
    public static final String SecretManagerProjectName = "IdentityProvider.SecretManager"

    public static class Docker {
        //Amount of IdP Server Apps scaled via Docker Engine
        public static final int ApiAppsCount = 4
        public static final String ApiServiceName = "idp-api"        
        public static final String ApiContainerName = "idp-api"
        public static final String ApiImageName = "idp-api"

        public static final String SecretManagerServiceName = "idp-SecretManager"        
        public static final String SecretManagerContainerName = "idp-SecretManager"
        public static final String SecretManagerImageName = "idp-SecretManager"

        public static final String NginxServiceName = "nginx-idp"
        public static final String NginxContainerName = "nginx-idp"
        public static final String NginxImageName = "nginx-idp"

        public static final String NginxNewRelicServiceName = "nginx-newrelic"
        public static final String NginxNewRelicContainerName = "nginx-newrelic"
        public static final String NginxNewRelicImageName = "nginx-newrelic"

        public static class ECR {            
            public static final String ApiRepoName = "identity-provider/idp-api"
            public static final String SecretManagerRepoName = "identity-provider/idp-SecretManager"
            public static final String NginxRepoName = "identity-provider/nginx-idp"
            public static final String NginxNewRelicRepoName = "identity-provider/nginx-newrelic"
            public static final String ImageNameTemplate = "\${awsAccountId}.dkr.ecr.\${awsRegion}.amazonaws.com/\${repoName}:\${imageTag}"
        }
    }
}

public class DockerImageNameData implements Serializable  {
    public String AwsAccountId
    public String AwsRegion
    public String ImageTag
}

public class RebuildAppAndCommitData implements Serializable  {
    public DockerizedAppRebuildData AppRebuildData
    public String ContainerName
    public String ServiceName
    public String OldDevEcrImageName
    public String NewDevEcrImageName
    public String ImageName
    public String ProjectName = ""
    public String TestProjectName = ""
}

class DockerizedAppRebuildData {
    boolean rebuildRequired = false
    boolean imagesExist = true
}

class DockerizedAppsRebuildData {
    DockerizedAppRebuildData api = new DockerizedAppRebuildData()
    DockerizedAppRebuildData SecretManager = new DockerizedAppRebuildData()
    DockerizedAppRebuildData nginx = new DockerizedAppRebuildData()
}

pipeline {
    agent any

    options {
        skipDefaultCheckout true
    }

    environment {
        PATH = "~/.local/bin:${env.PATH}"
    }

    stages {
        stage('User Input') {
            when {
                branch 'dev-jenkins'
            }
            steps {
                script{
                    createAwsKeysFile()
                }
            }
        }
        stage('Print useful info') {
            steps{
                script{
                    setupAwsCliKeys()
                    printUsefulInfo()
                }
            }
        }
        stage('SCM checkout') {
            steps{
                checkout([$class: 'GitSCM', branches: [[name: "*/${env.BRANCH_NAME}"]],
                    doGenerateSubmoduleConfigurations: false,
                    extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: SCMRelativeTargetDirectory]],
                    submoduleCfg: [],
                    userRemoteConfigs: [[credentialsId: 'Jenkins_SSH',
                        url: 'git@github.com:coretech/-identity-provider.git']]
                ])
            }
        }
        stage('Hello - dev') {
            when {
                branch 'dev'
            }
            steps {
                echo "Hello from dev"
            }
        }
        stage('Hello - master') {
            when {
                branch 'master'
            }
            steps {
                echo "Hello from master"
            }
        }
        stage('Hello - sandbox') {
            when {
                branch 'sandbox'
            }
            steps {
                echo "Hello from sandbox"
            }
        }
        stage('Hello - qa') {
            when {
                branch 'qa'
            }
            steps {
                echo "Hello from qa"
            }
        }
        stage('Start Docker service') {
            when {
                branch 'dev-jenkins'
            }
            steps {
                script{
                    startDockerService()
                }
            }
        }
        stage('Remove existing Docker containers/images') {
            when {
                branch 'dev-jenkins'
            }
            steps {
                script{
                    removeExistingDockerContainersAndImagesFromPreviousBuild()                    
                }
            }
        }                
        stage('Pull Docker images from previous Build') {
            when {
                branch 'dev-jenkins'
            }
            steps {
                script{
                   loginDockerClientToECR(AwsRegion, AwsAccountId)
                   pullDockerImagesFromPreviousBuild()
                }
            }
        }
        stage('Build Docker images/containers') {
            when {
                branch 'dev-jenkins'
            }
            steps {
                script {
                    def appsRebuildData = detectIfAppsRebuildRequired()
                    echo "From BUILD: identity Provider App Rebuild Required = ${appsRebuildData.api.rebuildRequired}"
                    echo "From BUILD: Cert Manager App Rebuild Required = ${appsRebuildData.SecretManager.rebuildRequired}"
                    echo "From BUILD: NGINX image exists from previous successful Jenkins build = ${appsRebuildData.nginx.imagesExist}"
                    rebuildAppsAndCommit(appsRebuildData)
                }
            }
        }
        stage('Push Docker images to ECR') {
            when {
                branch 'dev-jenkins'
            }
            steps {
                script {
                    pushImagesToECRs()
                }
            }
        }
        stage('Start Docker containers - final') {
            when {
                branch 'dev-jenkins'
            }
            steps {
                script {
                    startDockerContainersWithScaling(
                        SCMRelativeTargetDirectory, IdentityProviderConstants.Docker.ApiServiceName, IdentityProviderConstants.Docker.ApiAppsCount)
                }
            }
        }
    }
}

/******Common functions**********/

def printUsefulInfo() {
    def awsVersion = sh(script: "~/.local/bin/aws --version", returnStdout: true)
    echo "AWS Version: $awsVersion"    
    jenkinsUser = sh(script: 'whoami', returnStdout: true)
    hostIPs = sh(script: 'hostname -I', returnStdout: true)
    echo "Jenkins user: $jenkinsUser"
    echo "Jenkins host IPs: $hostIPs"
}

def runShellCommandOnDockerContainersByImages(List<String> imageNames, String shellCmd) {
    sh "docker ps -a | awk \'{ print \$1,\$2 }\' | grep -E \'${imageNames.join("|")}\' \
                     | awk \'{ print \$1 }\' | xargs -I % sh -c \'$shellCmd\'"
}

def removeDockerContainersByImages(List<String> imageNames) {
    runShellCommandOnDockerContainersByImages(imageNames, "docker stop % && docker rm %;")
}

def removeDockerImages(List<String> imageNames, boolean continueJenkinsJobExecutionAfterError = false) {
    sh "docker rmi -f ${imageNames.join(" ")} || $continueJenkinsJobExecutionAfterError"
}

def restartDockerContainersByImages(List<String> imageNames) {
    runShellCommandOnDockerContainersByImages(imageNames, "docker restart %;")
}

def startDockerContainersWithScaling(String composeDirectoryPath, String serviceToScale, int scaledContainersCount) {
    sh "cd $composeDirectoryPath && docker-compose up -d --no-recreate --scale $serviceToScale=$scaledContainersCount"
}

def startDockerService() {
    sh 'sudo service docker start'
}

def getStringByTemplate(String stringTemplate, Map binding) {
    def engine = new StrSubstitutor(binding)
    def resultedString = engine.replace(stringTemplate)
    return resultedString
}

def getBuildIdForJenkinsBuild(build) {
    return build?.getId()
}

def getLatestSuccessfulJenkinsBuild() {
    return currentBuild.rawBuild.getPreviousSuccessfulBuild()
}

def getCurrentJenkinsBuild() {
    return currentBuild.rawBuild
}

def getLatestSuccessfulCommit() {
    def latestSuccessfulHash = null
    def latestSuccessfulJenkinsBuild = getLatestSuccessfulJenkinsBuild()
    if (latestSuccessfulJenkinsBuild) {
        echo "Successful build was found"
        latestSuccessfulHash = getCommitHashForJenkinsBuild(latestSuccessfulJenkinsBuild)
    }
    return latestSuccessfulHash
}

/**
 * Gets the commit hash from a Jenkins build object, if any
 */
def getCommitHashForJenkinsBuild(Run build) {
    def hash = build.getAction(BuildData.class).lastBuiltRevision.sha1String;
    return hash;
}

def getListOfAffectedFilesSinceLatestSuccessfulBuild(String sourcePath){
    def fileNames = [] as String[]
    def latestSuccessfulCommit = getLatestSuccessfulCommit()

    if (latestSuccessfulCommit) {
        def currentCommit = getCommitHashForJenkinsBuild(currentBuild.rawBuild)
        fileNames = sh(
            script: "${sourcePath?.trim() ? "cd $sourcePath && " : ""}git diff --name-only $latestSuccessfulCommit $currentCommit",
            returnStdout: true
        ).split('\n')
        echo "Affected files between commits $latestSuccessfulCommit and $currentCommit are: ${fileNames}"
    }
    return fileNames
}

def createFilePath(String filePath) {
    if (env.NODE_NAME == null) {
        error "envvar NODE_NAME is not set, probably not inside an node {} or running an older version of Jenkins!";
    } 
    else {
        def jenkins = Jenkins.getInstance()        
        def fullPath = "${env.WORKSPACE}/$filePath"
        if(env.NODE_NAME == "master") {
            return new FilePath(new File(fullPath))
        } 
        else {
           return new FilePath(jenkins.getComputer(env.NODE_NAME).getChannel(), fullPath);
        }
    }
}

def createFileWithContent(String filePath, String fileContent) {
    def groovyFilePath = createFilePath(filePath)    
    if (groovyFilePath != null) {
        groovyFilePath.write(fileContent, null)
    }
}

def commitDockerContainer(String containerName, String imageName) {
    sh "docker commit $containerName $imageName"
}

def setupAwsCliKeys() {
    sh "~/.local/bin/aws configure set aws_access_key_id $DockerLoginAwsAccessKeyId && \
    ~/.local/bin/aws configure set aws_secret_access_key $DockerLoginAwsSecretAccessKey && \
    ~/.local/bin/aws configure set aws_session_token $DockerLoginAwsSessionToken && \
    ~/.local/bin/aws configure set region us-east-1"
}

def loginDockerClientToECR(String awsRegion, String awsAccountId) {
    sh "~/.local/bin/aws ecr get-login --no-include-email --region $awsRegion --registry-ids $awsAccountId | sh 2>&1"
}

def pullDockerImage(String imageName, boolean continueJenkinsJobExecutionAfterError = false) {
    sh "docker pull $imageName || $continueJenkinsJobExecutionAfterError"
}

def tagDockerImage(String imageName, String imageTag, boolean continueJenkinsJobExecutionAfterError = false) {
    sh "docker tag $imageName $imageTag || $continueJenkinsJobExecutionAfterError"
}

def pushDockerImage(String imageName) {
    sh "docker push $imageName"
}

def getShallowCopyOfObject(obj) {
    return obj.clone()
}

/******End of Common functions**********/

/******Only for DEV environment type usage**********/

def createAwsKeysFile() {
    DockerLoginAwsAccessKeyId = input message: 'Docker Login AWS access key setup',
        ok: 'Apply',
            parameters: [string(defaultValue: '', description: '', name: 'AWS access key')]

    DockerLoginAwsSecretAccessKey = input message: 'Docker Login AWS secret key setup',
        ok: 'Apply',
            parameters: [string(defaultValue: '', description: '', name: 'AWS secret key')]

    DockerLoginAwsSessionToken = input message: 'Docker Login AWS session token setup',
        ok: 'Apply',
            parameters: [string(defaultValue: '', description: '', name: 'AWS session token')]

    AwsAccessKeyId = input message: 'AWS access key setup',
        ok: 'Apply',
            parameters: [string(defaultValue: '', description: '', name: 'AWS access key')]

    AwsSecretAccessKey = input message: 'AWS secret key setup',
        ok: 'Apply',
            parameters: [string(defaultValue: '', description: '', name: 'AWS secret key')]

    AwsSessionToken = input message: 'AWS session token setup',
        ok: 'Apply',
            parameters: [string(defaultValue: '', description: '', name: 'AWS session token')]
    def fileContent = [AwsAccessKeyId, AwsSecretAccessKey, AwsSessionToken].join("\n")
    
    createFileWithContent(AwsKeysFileName, fileContent)
}

/******End of Only for DEV environment type usage**********/

    public DockerImageNameData getDockerImageNameData(String awsAccount, String awsRegion, boolean isNewImage = true) {
        def dockerImageNameData = new DockerImageNameData(
                                     AwsAccountId: awsAccount,
                                     AwsRegion: awsRegion,
                                     ImageTag: isNewImage ? "${env.BRANCH_NAME}-${getBuildIdForJenkinsBuild(getCurrentJenkinsBuild())}" : 
                                            "${env.BRANCH_NAME}-${getBuildIdForJenkinsBuild(getLatestSuccessfulJenkinsBuild())}"
                                  )
        return dockerImageNameData
    }

    public LinkedHashMap getImagesData(DockerImageNameData imageNameData) {
       def binding = [ awsAccountId: imageNameData.AwsAccountId, awsRegion: imageNameData.AwsRegion,
                       imageTag: imageNameData.ImageTag]
       def ecrIdentityProviderBinding = binding.clone()
       def ecrSecretManagerBinding = binding.clone()
       def ecrNginxBinding = binding.clone()
       def ecrNginxNewRelicBinding = binding.clone()
       ecrIdentityProviderBinding.repoName = IdentityProviderConstants.Docker.ECR.ApiRepoName
       ecrSecretManagerBinding.repoName = IdentityProviderConstants.Docker.ECR.SecretManagerRepoName
       ecrNginxBinding.repoName = IdentityProviderConstants.Docker.ECR.NginxRepoName
       ecrNginxNewRelicBinding.repoName = IdentityProviderConstants.Docker.ECR.NginxNewRelicRepoName

       def imagesData =  [api : [ecrImageName : getStringByTemplate(IdentityProviderConstants.Docker.ECR.ImageNameTemplate, ecrIdentityProviderBinding)],
                          SecretManager : [ecrImageName : getStringByTemplate(IdentityProviderConstants.Docker.ECR.ImageNameTemplate, ecrSecretManagerBinding)],
                          nginx : [ecrImageName : getStringByTemplate(IdentityProviderConstants.Docker.ECR.ImageNameTemplate, ecrNginxBinding)],
                          nginxNewRelic : [ecrImageName : getStringByTemplate(IdentityProviderConstants.Docker.ECR.ImageNameTemplate, ecrNginxNewRelicBinding)]                          
                        ]
        return imagesData
    }

def pullDockerImagesFromPreviousBuild() {    
    def latestSuccessfulJenkinsBuildId = getBuildIdForJenkinsBuild(getLatestSuccessfulJenkinsBuild())
    if(latestSuccessfulJenkinsBuildId) {
        pullDockerImages(getDockerImageNameData(AwsAccountId, AwsRegion, false), true)
    }
}

void pullDockerImages(DockerImageNameData dockerImageNameData, boolean continueExecutionAfterFail) {
    def imagesData = getImagesData(dockerImageNameData)

    parallel(
       PullApi: { 
          sh "docker pull ${imagesData.api.ecrImageName} || $continueExecutionAfterFail" 
       },
       PullSecretManager: { 
          sh "docker pull ${imagesData.SecretManager.ecrImageName} || $continueExecutionAfterFail"
       },
       PullNginx: {
          sh "docker pull ${imagesData.nginx.ecrImageName} || $continueExecutionAfterFail"
       }
    )
}

def removeExistingDockerContainersAndImagesFromPreviousBuild() {
    sh "cd $SCMRelativeTargetDirectory && docker-compose down"
    def latestSuccessfulJenkinsBuildId = getBuildIdForJenkinsBuild(getLatestSuccessfulJenkinsBuild())
    if(latestSuccessfulJenkinsBuildId) {
        def imagesData = getImagesData(getDockerImageNameData(AwsAccountId, AwsRegion, false))
        def imageNames = [imagesData.api.ecrImageName,  imagesData.SecretManager.ecrImageName,  imagesData.nginx.ecrImageName]

        removeDockerContainersByImages(imageNames)
        removeDockerImages(imageNames + [IdentityProviderConstants.Docker.ApiImageName, 
        IdentityProviderConstants.Docker.SecretManagerImageName,
        IdentityProviderConstants.Docker.NginxImageName], true)
    }                    
}

String getPublishBashScript(boolean withRebuild, RebuildAppAndCommitData rebuildAppAndCommitData) {
    def projectName = rebuildAppAndCommitData.ProjectName
    def projectPath = "src/$projectName/${projectName}.csproj"
    def commonProjectPath = "src/${IdentityProviderConstants.CommonProjectName}/${IdentityProviderConstants.CommonProjectName}.csproj"
    def publishBashScript = "cd .. && ${withRebuild ? "dotnet clean $projectPath && \
    dotnet clean $commonProjectPath && " : ""} rm -rf /app/* && \
    dotnet publish ${withRebuild ? "" : "--no-build"} $projectPath -c Release -o /app"
    if(rebuildAppAndCommitData.TestProjectName?.trim()) {
        publishBashScript += " && dotnet restore src/${rebuildAppAndCommitData.TestProjectName}/${rebuildAppAndCommitData.TestProjectName}.csproj"
    }
    if(projectName == IdentityProviderConstants.ApiProjectName) {
        def runApiScriptPath = "/src/IdentityProvider.API/run_api.sh"
        publishBashScript += " && chmod +x $runApiScriptPath && dos2unix $runApiScriptPath"
    }
    return publishBashScript
}

void publishAppInsideOfContainer(boolean withRebuild, RebuildAppAndCommitData rebuildAppAndCommitData) {
    def projectName = rebuildAppAndCommitData.ProjectName
    def containerName = rebuildAppAndCommitData.ContainerName
    
    def publishBashScript = getPublishBashScript(withRebuild, rebuildAppAndCommitData)
    def shellScript = "${withRebuild ? "docker exec $containerName bash -c \'rm -rf ../src/*\' && " : ""}\
    docker cp ${SCMRelativeTargetDirectory}/$projectName/. $containerName:/src/$projectName && \
    docker cp ${SCMRelativeTargetDirectory}/${IdentityProviderConstants.CommonProjectName}/. $containerName:/src/${IdentityProviderConstants.CommonProjectName} && \
    ${rebuildAppAndCommitData.TestProjectName?.trim() ? "docker cp ${SCMRelativeTargetDirectory}/${rebuildAppAndCommitData.TestProjectName}/. $containerName:/src/${rebuildAppAndCommitData.TestProjectName} && " : ""} \
    docker exec $containerName bash -c \'$publishBashScript\'"
    sh "$shellScript"
}

void updateContainerByGitChangesAndCommit(boolean withRebuild,                                          
                                          RebuildAppAndCommitData rebuildAppAndCommitData) {
    def containerName = rebuildAppAndCommitData.ContainerName
    if(containerName == IdentityProviderConstants.Docker.NginxContainerName) {
        sh "cd ${SCMRelativeTargetDirectory}/nginx && \
        docker cp nginx.conf $containerName:/etc/nginx/nginx.conf && \
        docker cp run_nginx.sh $containerName:/tmp/run_nginx.sh && \
        docker exec -d $containerName bash -c 'chmod +x /tmp/run_nginx.sh && dos2unix /tmp/run_nginx.sh && \
        ln -sf /dev/stdout /etc/nginx/nginx.access.log && ln -sf /dev/stderr /etc/nginx/nginx.error.log'"
    }
    else {
        publishAppInsideOfContainer(withRebuild, rebuildAppAndCommitData) 
    }

    sh "docker commit $containerName ${rebuildAppAndCommitData.NewDevEcrImageName}"
    sh "docker stop $containerName && docker rm $containerName && docker rmi -f ${rebuildAppAndCommitData.OldDevEcrImageName}"
}

void rebuildAppAndCommit(RebuildAppAndCommitData rebuildAppAndCommitData) {
    if (rebuildAppAndCommitData.AppRebuildData.rebuildRequired) {
        if (!rebuildAppAndCommitData.AppRebuildData.imagesExist) {
            sh "cd $SCMRelativeTargetDirectory && docker-compose build ${rebuildAppAndCommitData.ServiceName} && \
            docker tag ${rebuildAppAndCommitData.ImageName} ${rebuildAppAndCommitData.NewDevEcrImageName}"
        }
        else {
            updateContainerByGitChangesAndCommit(true, rebuildAppAndCommitData)
        }
    }
    else {
        updateContainerByGitChangesAndCommit(false, rebuildAppAndCommitData)
    }
    if(rebuildAppAndCommitData.ContainerName != IdentityProviderConstants.Docker.NginxContainerName) {
        sh "docker run -d --name=${rebuildAppAndCommitData.ContainerName} ${rebuildAppAndCommitData.NewDevEcrImageName} tail -f /dev/null && \
        docker cp $AwsKeysFileName ${rebuildAppAndCommitData.ContainerName}:/app/$AwsKeysFileName && \
        docker commit ${rebuildAppAndCommitData.ContainerName} ${rebuildAppAndCommitData.NewDevEcrImageName}-new && \
        docker stop ${rebuildAppAndCommitData.ContainerName} && docker rm ${rebuildAppAndCommitData.ContainerName} && \
        docker rmi -f ${rebuildAppAndCommitData.NewDevEcrImageName} && \
        docker tag ${rebuildAppAndCommitData.NewDevEcrImageName}-new ${rebuildAppAndCommitData.NewDevEcrImageName} && \
        docker tag ${rebuildAppAndCommitData.NewDevEcrImageName} ${rebuildAppAndCommitData.ImageName} && \
        docker rmi -f ${rebuildAppAndCommitData.NewDevEcrImageName}-new"
    }
}

void rebuildAppsAndCommit(DockerizedAppsRebuildData appsRebuildData) {
    def newQaImagesData = getImagesData(getDockerImageNameData(AwsAccountId, AwsRegion))
    def oldQaImagesData = getImagesData(getDockerImageNameData(AwsAccountId, AwsRegion, false))
    
    parallel(
        RebuildApi: {
            def rebuildAppAndCommitData = new RebuildAppAndCommitData(
                AppRebuildData: appsRebuildData.api,
                ContainerName: IdentityProviderConstants.Docker.ApiContainerName,
                ServiceName: IdentityProviderConstants.Docker.ApiServiceName,
                OldDevEcrImageName: oldQaImagesData.api.ecrImageName,
                NewDevEcrImageName: newQaImagesData.api.ecrImageName,
                ImageName: IdentityProviderConstants.Docker.ApiImageName,
                ProjectName: IdentityProviderConstants.ApiProjectName,
                TestProjectName: IdentityProviderConstants.ApiTestProjectName
            )
            rebuildAppAndCommit(rebuildAppAndCommitData)
        },
        RebuildSecretManager: {
            def rebuildAppAndCommitData = new RebuildAppAndCommitData(
                AppRebuildData: appsRebuildData.SecretManager,
                ContainerName: IdentityProviderConstants.Docker.SecretManagerContainerName,
                ServiceName: IdentityProviderConstants.Docker.SecretManagerServiceName,
                OldDevEcrImageName: oldQaImagesData.SecretManager.ecrImageName,
                NewDevEcrImageName: newQaImagesData.SecretManager.ecrImageName,
                ImageName: IdentityProviderConstants.Docker.SecretManagerImageName,
                ProjectName: IdentityProviderConstants.SecretManagerProjectName
            )
            rebuildAppAndCommit(rebuildAppAndCommitData)
        },
        RebuildNginx: {
            def rebuildAppAndCommitData = new RebuildAppAndCommitData(
                AppRebuildData: appsRebuildData.nginx,
                ContainerName: IdentityProviderConstants.Docker.NginxContainerName,
                ServiceName: IdentityProviderConstants.Docker.NginxServiceName,
                OldDevEcrImageName: oldQaImagesData.nginx.ecrImageName,
                NewDevEcrImageName: newQaImagesData.nginx.ecrImageName,
                ImageName: IdentityProviderConstants.Docker.NginxImageName
            )
            rebuildAppAndCommit(rebuildAppAndCommitData)
        }
    )    
}

void pushImagesToECRs() {    
    LinkedHashMap imagesData = [:]
    def pushImages = {
        parallel(
            PushApi: {
               sh "docker tag ${imagesData.api.ecrImageName} ${IdentityProviderConstants.Docker.ApiImageName} && docker push ${imagesData.api.ecrImageName}"
            },
            PushSecretManager: {
               sh "docker tag ${imagesData.SecretManager.ecrImageName} ${IdentityProviderConstants.Docker.SecretManagerImageName} && docker push ${imagesData.SecretManager.ecrImageName} && docker rmi -f ${imagesData.SecretManager.ecrImageName}"
            },
            PushNginx: {
               sh "docker tag ${imagesData.nginx.ecrImageName} ${IdentityProviderConstants.Docker.NginxImageName} && docker push ${imagesData.nginx.ecrImageName} && docker rmi -f ${imagesData.nginx.ecrImageName}"
            }
        )
    }

    imagesData = getImagesData(getDockerImageNameData(AwsAccountId, AwsRegion))
    pushImages()
}

DockerizedAppsRebuildData detectIfAppsRebuildRequired() {
    def appsRebuildData = new DockerizedAppsRebuildData()    
    def imagesData = getImagesData(getDockerImageNameData(AwsAccountId, AwsRegion, false))

    if(getLatestSuccessfulJenkinsBuild()) {
        def imageNamesToFind = [ imagesData.api.ecrImageName, imagesData.SecretManager.ecrImageName, 
        imagesData.nginx.ecrImageName, imagesData.nginxNewRelic.ecrImageName ]
        def imageNames = sh(
        script: "docker image ls --format \"{{.Repository}}:{{.Tag}}\" --all | grep -E \'${imageNamesToFind.join("|")}\' \
        | awk \'{ print \$1 }\'",
        returnStdout: true
        )
        if (imageNames?.trim()) {
            imageNames = imageNames.split('\n')
            appsRebuildData.api.imagesExist = imageNames.any 
            { imageName -> imageName == imagesData.api.ecrImageName }
            appsRebuildData.SecretManager.imagesExist = imageNames.any 
            { imageName -> imageName == imagesData.SecretManager.ecrImageName }
            appsRebuildData.nginx.imagesExist = imageNames.any 
            { imageName -> imageName == imagesData.nginx.ecrImageName }
        }
        else {
            appsRebuildData.api.imagesExist = false
            appsRebuildData.SecretManager.imagesExist = false
            appsRebuildData.nginx.imagesExist = false
            //appsRebuildData.nginxNewRelic.imagesExist = false
        }
    }
    else {
       appsRebuildData.api.imagesExist = false
       appsRebuildData.SecretManager.imagesExist = false
       appsRebuildData.nginx.imagesExist = false
       //appsRebuildData.nginxNewRelic.imagesExist = false
    }

    if (!appsRebuildData.api.imagesExist) {
        appsRebuildData.api.rebuildRequired = true
        echo "Docker image ${imagesData.api.ecrImageName} was not found, so code rebuild is required"
    }
    else {
        sh "docker stop ${IdentityProviderConstants.Docker.ApiContainerName} && \
        docker rm ${IdentityProviderConstants.Docker.ApiContainerName} || true"
        sh "docker run -d --name=${IdentityProviderConstants.Docker.ApiContainerName} \
        ${imagesData.api.ecrImageName} tail -f /dev/null"
    }

    if (!appsRebuildData.SecretManager.imagesExist) {
        appsRebuildData.SecretManager.rebuildRequired = true
        echo "Docker image ${imagesData.SecretManager.ecrImageName} was not found, so code rebuild is required"
    }
    else {
        sh "docker stop ${IdentityProviderConstants.Docker.SecretManagerContainerName} && \
        docker rm ${IdentityProviderConstants.Docker.SecretManagerContainerName} || true"
        sh "docker run -d --name=${IdentityProviderConstants.Docker.SecretManagerContainerName} \
        ${imagesData.SecretManager.ecrImageName} tail -f /dev/null"
    }

    if (!appsRebuildData.nginx.imagesExist) {
        appsRebuildData.nginx.rebuildRequired = true
        echo "Docker image ${imagesData.nginx.ecrImageName} was not found, so this image should be built from scratch"
    }
    else {
        sh "docker stop ${IdentityProviderConstants.Docker.NginxContainerName} && \
        docker rm ${IdentityProviderConstants.Docker.NginxContainerName} || true"
        sh "docker run -d --name=${IdentityProviderConstants.Docker.NginxContainerName} \
        ${imagesData.nginx.ecrImageName} tail -f /dev/null"
    }
/*
    if (!appsRebuildData.nginxNewRelic.imagesExist) {
        appsRebuildData.nginxNewRelic.rebuildRequired = true
        echo "Docker image ${imagesData.nginxNewRelic.ecrImageName} was not found, so this image should be built from scratch"
    }
    else {
        sh "docker stop ${IdentityProviderConstants.Docker.NginxNewRelicContainerName} && \
        docker rm ${IdentityProviderConstants.Docker.NginxNewRelicContainerName} || true"
        sh "docker run -d --name=${IdentityProviderConstants.Docker.NginxNewRelicContainerName} \
        ${imagesData.nginxNewRelic.ecrImageName} tail -f /dev/null"
    }
*/
    if (appsRebuildData.api.rebuildRequired && appsRebuildData.SecretManager.rebuildRequired) {
        return appsRebuildData
    }

    def fileNames = getListOfAffectedFilesSinceLatestSuccessfulBuild(SCMRelativeTargetDirectory)
    for (String fileName in fileNames) {
        if (fileName.endsWith(".cs") || fileName.endsWith(".csproj")) {
            if (fileName.startsWith("${IdentityProviderConstants.ApiProjectName}/")) {
                appsRebuildData.api.rebuildRequired = true                
            }
            else if (fileName.startsWith("${IdentityProviderConstants.SecretManagerProjectName}/")) {
                appsRebuildData.SecretManager.rebuildRequired = true                
            }
            else if (fileName.startsWith("${IdentityProviderConstants.CommonProjectName}/")) {
                appsRebuildData.api.rebuildRequired = true
                appsRebuildData.SecretManager.rebuildRequired = true
                break;
            }
            if (appsRebuildData.api.rebuildRequired && appsRebuildData.SecretManager.rebuildRequired) {
                break;
            }
        }
    }
    return appsRebuildData
}