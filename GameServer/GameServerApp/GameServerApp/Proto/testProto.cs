//***********************************************************
// 描述：
// 作者：郭金宝 
// 创建时间：2025-05-22 14:56:45
// 备注：
//***********************************************************
//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2025-05-22 14:55:15
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 测试协议
/// </summary>
public struct testProto : IProto
{
    public ushort ProtoCode { get { return 17001; } }

    public int Id; //编号
    public string Name; //名字
    public int Age; //年龄

    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteInt(Id);
            ms.WriteUTF8String(Name);
            ms.WriteInt(Age);
            return ms.ToArray();
        }
    }

    public static testProto GetProto(byte[] buffer)
    {
        testProto proto = new testProto();
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
            proto.Id = ms.ReadInt();
            proto.Name = ms.ReadUTF8String();
            proto.Age = ms.ReadInt();
        }
        return proto;
    }
}