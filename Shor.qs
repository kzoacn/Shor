namespace ShorAlgo
{
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Extensions.Math;
    open Microsoft.Quantum.Extensions.Diagnostics;
    function PowerOfTwo(k : Int) : (Double) {
        mutable ans=1.0;
        for(i in 0..k-1){
            set ans=ans*2.0;
        }
        return ans;
    }


    operation Ux(x : Qubit[], a : Int , N : Int):(){
        //|x>  ->  |(ax mod N)>
        body{
            let qs=LittleEndian(x);
            ModularMultiplyByConstantLE(a,N,qs);
        }
        controlled auto;
    }
    
    operation myQFT (qs : Qubit[]) : ()
    {
        body{
            let n=Length(qs);
            mutable exp2=new Double[n+1];
            let pi=PI();
            for(i in 0..n){
                set exp2[i]=PowerOfTwo(i);
            }
            for(i in 0..n-1){
                H(qs[i]);
                for(j in i+1..n-1){
                    (Controlled R1)([qs[j]],(2.0*pi/exp2[j-i+1],qs[i]));
                }
            } 
            for(i in 0..n/2-1){
                SWAP(qs[i],qs[n-1-i]);
            }
        }
        adjoint {
        
            let n=Length(qs);
            mutable exp2=new Double[n+1];
            let pi=PI();
            for(i in 0..n){
                set exp2[i]=PowerOfTwo(i);
            }

            for(i in 0..n/2-1){
                SWAP(qs[i],qs[n-1-i]);
            }
            for(i in 0..n-1){
                let x=n-1-i;
                for(j in x+1..n-1){
                    let y=(n+x)-j;
                    (Controlled R1)([qs[y]],(-2.0*pi/exp2[y-x+1],qs[x]));
                }
                H(qs[x]);
            } 

        }
    }

    function modExp(xx : Int , kk : Int , N : Int):(Int){
            mutable k=kk;
            mutable ans=1;
            mutable x=xx;
            mutable two=1;
            for(i in 0..20){
                if(k%2==1){
                    set ans=ans*x%N;
                }
                set x=x*x%N;
                set k=k/2;
            }
            return ans;
    }
    function Exp2(x : Int , k : Int):(Int){
            mutable ans=1;
            for(i in 0..k-1){
                set ans=ans*x;
            }
            return ans;
    }

    operation OrderFinding(a : Int, N : Int):(Int[]){
        body{
            let L=5;
            let t=7;
            mutable res=new Int[t];
            using(qs=Qubit[L+t]){
                let x=qs[0..t-1];
                let y=qs[t..L+t-1];


                for(i in 0..t-1){
                    H(x[i]);
                }

                
                X(y[0]);

                for(i in 0..t-1){
                    let r=t-1-i;
                    (Controlled Ux)([x[i]],(y,modExp(a,Exp2(2,r),N),N));
                }
                //Ux(y,modExp(a,2,N),N);
                
                //DumpMachine("deb.txt");

                //for(i in 0..t/2-1){
                //    SWAP(x[i],x[t-1-i]);
                //}

                (Adjoint myQFT)(x);
                //(Adjoint QFT)(BigEndian(x));
                

                for(i in 0..t-1){
                    if(M(x[i])==One){
                        set res[i]=1;
                    }else{
                        set res[i]=0;
                    }
                }
                ResetAll(qs);
            }
            return res;
        }
    }
    operation MyTest() : (Int[]) {
        body{

            return OrderFinding(5,21);   
        }
    }
    operation FindSDivideR(a : Int ,N : Int) : (Int[]){
        body{
            return OrderFinding(a,N);   
        }
    }

    operation TestUx() : () {
        body{
            Message("hi");
            using(qs=Qubit[4]){
                X(qs[2]);
                X(qs[1]);
                Ux(qs,2,15);
                DumpMachine("");
                ResetAll(qs);
            }
        }
    }
}
