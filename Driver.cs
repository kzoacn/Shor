using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
namespace Shor
{
    using ShorAlgo;
    class Driver
    {

        static long  gcd(long a,long b){
            if(b==0)
                return a;
            return gcd(b,a%b);
        }

        static long  OrderFinding(long x,long N){
            long e=1;
            for(int i=1;i<N;i++){
                e=e*x%N;
                if(e==1)
                    return i;
            }
            return -1;
        }
        static long modExp(long x,long k,long N){
            long ans=1;
            for(;k>0;k>>=1){
                if(k%2>0)
                    ans=ans*x%N;
                x=x*x%N;
            }
            return ans;
        }
        static double findsr(long a,long N){
            double sr=0;
            double t=1.0;
            int tbit=7;
            using (var sim = new QuantumSimulator())
            {
                var res = FindSDivideR.Run(sim,a,N).Result;
                //var res=TestUx.Run(sim).Result;
                for(int i=0;i<tbit;i++){
                    t=t*2.0;
                    if(res[i]==1){
                        sr=sr*2+1;
                    }else{
                        sr=sr*2;
                    }
                }
                sr=sr/t;
                //for(int i=0;i<tbit;i++)
                //    Console.Write($"{res[i]}");
            }       
            return sr;   
        }
        class Frac{
            public long son,mom;
            public Frac(){son=0;mom=1;}
            public Frac(long _son,long _mom){
                son=_son;mom=_mom;
            }
            public Frac norm(){
                long d=gcd(son,mom);
                son/=d;
                mom/=d;
                return this;
            }
            public Frac add(Frac oth){
                Frac f=new Frac();
                f.mom=mom*oth.mom;
                f.son=mom*oth.son+oth.mom*son;
                f.norm();
                return f;
            }
            public Frac inverse(){
                return new Frac(mom,son);
            }

        }
        static bool check(long a,long r,long N){
            Console.WriteLine($"Test (a={a})^{r} =1 (mod N={N})");
            return modExp(a,r,N)==1;
        }
        static Frac getFrac(long[] a,int n){
            Frac ans=new Frac(1,a[n]);
            for(int i=n-1;i>=1;i--){
                ans=ans.add(new Frac(a[i],1));
                ans=ans.inverse();
            }
            return ans;
        }
        static long QOrderFinding(long a,long N){
            while(true){
                double sr=findsr(a,N);
                Console.WriteLine($"find s/r={sr}");
                long[] arr=new long[100];
                int n=0;
                while(sr>1e-6){
                    sr=1/sr;
                    n++;
                    arr[n]=(long)(sr+1e-9);
                    sr=sr-((long)(sr+1e-9));
                }
                Console.WriteLine("Continued Fraction is ");
                for(int i=0;i<=n;i++)
                    Console.Write($"{arr[i]},");
                Console.WriteLine("");

                for(int i=1;i<=n;i++){
                    Frac frac=getFrac(arr,i);
                    Console.WriteLine($"Get Frac {frac.son}/{frac.mom}");
                    if(check(a,frac.mom,N)){
                        return frac.mom;
                    }
                }
            }
        }
        static long factor(long N){
            Random random = new Random(3);
            while(true){
                long x=random.Next((int)N);
                if(x==0 || x==1 || x==N-1)continue;
                System.Console.WriteLine($"Try N={N} x={x}");
                long d=gcd(x,N);
                if(d>1){
                    continue;
                    System.Console.WriteLine("get factor by random");
                    return d;
                }else{
                    long r=QOrderFinding(x,N);
                    if(r%2!=0)continue;
                    System.Console.WriteLine($"r={r}");
                    long d1=(modExp(x,r/2,N)-1)%N;
                    long d2=(modExp(x,r/2,N)+1)%N;
                    System.Console.WriteLine($"d1={d1} d2={d2}");
                    d1=gcd(d1,N);
                    d2=gcd(d2,N);
                    if(d1==N || d2==N)continue;
                    System.Console.WriteLine("get factor by order finding");

                    return d1>1 ? d1 : d2;
                }
            }
        }
        static void Main(string[] args)
        {
            
            long p1=3,p2=7;
            long N=p1*p2;//N<31
            long p=factor(N);
            System.Console.WriteLine($"p={p} q={N/p}");
        }
    }
}