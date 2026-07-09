pipeline {

    agent any

    environment {

        PATH = "/Users/williamgiftson/.dotnet/tools:/usr/local/share/dotnet:/opt/homebrew/opt/openjdk@21/bin:${env.PATH}"

        JAVA_HOME = "/opt/homebrew/opt/openjdk@21"

        SONAR_TOKEN = credentials('sonar-token')
    }


    stages {


        stage('Checkout') {

            steps {

                git branch: 'main',
                    credentialsId: 'github-creds',
                    url: 'https://github.com/WLiam18/RoadReady.git'
            }
        }



        stage('Restore') {

            steps {

                sh '''
                export JAVA_HOME=/opt/homebrew/opt/openjdk@21
                export PATH=$JAVA_HOME/bin:$PATH

                dotnet restore RoadReady.slnx
                '''
            }
        }



        stage('Check Environment') {

            steps {

                sh '''
                echo "===== USER ====="
                whoami


                echo "===== DOTNET ====="
                which dotnet
                dotnet --version


                echo "===== SONAR SCANNER ====="
                which dotnet-sonarscanner


                echo "===== JAVA ====="
                export JAVA_HOME=/opt/homebrew/opt/openjdk@21
                export PATH=$JAVA_HOME/bin:$PATH

                echo $JAVA_HOME
                which java
                java -version
                '''
            }
        }



        stage('SonarQube Analysis') {

            steps {

                withSonarQubeEnv('SonarQube') {

                    sh '''
                    export JAVA_HOME=/opt/homebrew/opt/openjdk@21
                    export PATH=$JAVA_HOME/bin:$PATH


                    echo "Starting Sonar Analysis"


                    dotnet sonarscanner begin \
                    /k:"RoadReady" \
                    /d:sonar.host.url="http://localhost:9000" \
                    /d:sonar.login=$SONAR_TOKEN



                    dotnet build RoadReady.slnx --no-restore



                    dotnet sonarscanner end \
                    /d:sonar.login=$SONAR_TOKEN


                    echo "Sonar Analysis Completed"
                    '''
                }
            }
        }



        stage('Test') {

            steps {

                sh '''
                echo "Running Tests"

                dotnet test RoadReady.slnx --no-restore

                echo "Tests Completed"
                '''
            }
        }

    }
}